// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.09.09

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Rse;
using FieldInfo=Xtensive.Storage.Model.FieldInfo;

namespace Xtensive.Storage.Internals
{
  [Serializable]
  internal sealed class EntitySetPrefetchTask
  {
    private static readonly object itemsQueryCachingRegion = new object();
    private static readonly Parameter<Tuple> ownerParameter = new Parameter<Tuple>(WellKnown.KeyFieldName);
    private static readonly Parameter<int> itemCountLimitParameter = new Parameter<int>("ItemCountLimit");
    private static readonly MethodInfo getValueMethodDefinition = typeof (Tuple)
      .GetMethods(BindingFlags.Public | BindingFlags.Instance)
      .Where(method => method.Name=="GetValue" && method.GetParameters().Length == 1
        && method.IsGenericMethodDefinition).Single();

    private readonly Key ownerKey;
    private readonly PrefetchProcessor processor;
    private QueryTask itemsQueryTask;
    private int? cachedHashCode;

    public readonly FieldInfo ReferencingField;

    public List<Tuple> Result { get { return itemsQueryTask.Result; } }

    public RecordSet RecordSet { get; private set; }

    public int? ItemCountLimit { get; private set; }

    public bool IsActive { get { return itemsQueryTask!=null; } }

    public void RegisterQueryTask()
    {
      EntitySetState state;
      if (processor.Owner.TryGetEntitySetState(ownerKey, ReferencingField, out state)
        && (state.IsFullyLoaded || state.Count >= ItemCountLimit))
        return;
      itemsQueryTask = CreateQueryTask();
      processor.Owner.Session.RegisterDelayedQuery(itemsQueryTask);
    }

    public bool Equals(EntitySetPrefetchTask other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (ReferenceEquals(this, other))
        return true;
      if (!ReferencingField.Equals(other.ReferencingField))
        return false;
      return (ItemCountLimit == null && other.ItemCountLimit == null)
        // TODO: This check should be removed (when the handling of TakeProvider in cached queries will be fixed)
        || (ItemCountLimit == other.ItemCountLimit);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      var otherType = obj.GetType();
      if (otherType != (typeof (EntitySetPrefetchTask)))
        return false;
      return Equals((EntitySetPrefetchTask) obj);
    }

    public override int GetHashCode()
    {
      if (cachedHashCode==null)
        unchecked {
          cachedHashCode = (ReferencingField.GetHashCode() * 397)
            ^ (ItemCountLimit.HasValue ? ItemCountLimit.Value : 0);
        }
      return cachedHashCode.Value;
    }

    private QueryTask CreateQueryTask()
    {
      var parameterContext = new ParameterContext();
      using (parameterContext.Activate()) {
        ownerParameter.Value = ownerKey.Value;
        if (ItemCountLimit != null)
          itemCountLimitParameter.Value = ItemCountLimit.Value;
        RecordSet = (RecordSet) processor.Owner.Session.Domain.GetCachedItem(
          new Pair<object, EntitySetPrefetchTask>(itemsQueryCachingRegion, this), CreateRecordSetLoadingItems);
        var executableProvider = CompilationContext.Current.Compile(RecordSet.Provider);
        return new QueryTask(executableProvider, parameterContext);
      }
    }

    private static RecordSet CreateRecordSetLoadingItems(object cachingKey)
    {
      var pair = (Pair<object, EntitySetPrefetchTask>) cachingKey;
      var associationIndex = pair.Second.ReferencingField.Association.UnderlyingIndex;
      var primaryTargetIndex = pair.Second.ReferencingField.Association.TargetType.Indexes.PrimaryIndex;
      var foreignKeyColumns = GetKeyColumnIndexes(associationIndex);
      var primaryKeyColumns = GetKeyColumnIndexes(primaryTargetIndex);
      var joiningColumns = GetJoiningColumnIndexes(associationIndex, primaryKeyColumns);
      var resultColumns = new List<int>(primaryTargetIndex.Columns.Count);
      if (pair.Second.ReferencingField.Association.AuxiliaryType == null)
        AddResultColumnIndexes(resultColumns, primaryTargetIndex, associationIndex.Columns.Count);
      else {
        AddResultColumnIndexes(resultColumns, associationIndex, 0);
        AddResultColumnIndexes(resultColumns, primaryTargetIndex, resultColumns.Count);
      }
      ParameterExpression tupleParameter;
      var filterExpression = CreateFilterExpression(associationIndex, foreignKeyColumns, out tupleParameter);
      var result = associationIndex.ToRecordSet()
        .Filter(Expression.Lambda<Func<Tuple, bool>>(filterExpression, tupleParameter))
        .Alias("a").Join(primaryTargetIndex.ToRecordSet(), joiningColumns);
      if (pair.Second.ItemCountLimit != null)
        result = result.Take(() => itemCountLimitParameter.Value);
      return result.Select(resultColumns.ToArray());
    }

    private static void AddResultColumnIndexes(ICollection<int> indexes, IndexInfo index,
      int columnIndexOffset)
    {
     for (int i = 0; i < index.Columns.Count; i++) {
        var column = index.Columns[i];
        if (PrefetchTask.IsFieldIntrinsicNonLazy(column.Field))
          indexes.Add(i + columnIndexOffset);
      }
    }

    private static Pair<int>[] GetJoiningColumnIndexes(IndexInfo associationIndex, int[] primaryKeyColumns)
    {
      var joiningColumns = new Pair<int>[primaryKeyColumns.Length];
      for (int i = 0; i < joiningColumns.Length; i++)
        joiningColumns[i] =
          new Pair<int>(associationIndex.Columns.IndexOf(associationIndex.ValueColumns[i]),
            primaryKeyColumns[i]);
      return joiningColumns;
    }

    private static Expression CreateFilterExpression(IndexInfo associationIndex,
      int[] foreignKeyColumns, out ParameterExpression tupleParameter)
    {
      Expression filterExpression = null;
      tupleParameter = Expression.Parameter(typeof (Tuple), "tuple");
      var valueProperty = typeof (Parameter<Tuple>).GetProperty("Value", typeof (Tuple));
      var keyValue = Expression.Property(Expression.Constant(ownerParameter), valueProperty);
      for (int i = 0; i < foreignKeyColumns.Length; i++) {
        var getValueMethod = getValueMethodDefinition
          .MakeGenericMethod(associationIndex.Columns[foreignKeyColumns[i]].ValueType);
        var tupleParameterFieldAccess = Expression.Call(tupleParameter, getValueMethod,
          Expression.Constant(foreignKeyColumns[i]));
        var ownerKeyFieldAccess = Expression.Call(keyValue, getValueMethod,
          Expression.Constant(i));
        if (filterExpression == null)
          filterExpression = Expression.Equal(tupleParameterFieldAccess, ownerKeyFieldAccess);
        else
          filterExpression = Expression.And(filterExpression,
            Expression.Equal(tupleParameterFieldAccess, ownerKeyFieldAccess));
      }
      return filterExpression;
    }

    private static int[] GetKeyColumnIndexes(IndexInfo index)
    {
      var result = new int[index.KeyColumns.Count];
      var firstColumnIndex = index.Columns.IndexOf(index.KeyColumns[0].Key);
      for (int i = 0; i < result.Length; i++)
        result[i] = firstColumnIndex + i;
      return result;
    }


    // Constructors

    public EntitySetPrefetchTask(Key ownerKey, PrefetchFieldDescriptor referencingFieldDescriptor, PrefetchProcessor processor)
    {
      ArgumentValidator.EnsureArgumentNotNull(ownerKey, "ownerKey");
      ArgumentValidator.EnsureArgumentNotNull(referencingFieldDescriptor, "referencingFieldDescriptor");
      ArgumentValidator.EnsureArgumentNotNull(processor, "processor");

      this.ownerKey = ownerKey;
      ReferencingField = referencingFieldDescriptor.Field;
      ItemCountLimit = referencingFieldDescriptor.EntitySetItemCountLimit;
      this.processor = processor;
    }
  }
}
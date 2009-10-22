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

    public RecordSet RecordSet { get; private set; }

    public int? ItemCountLimit { get; private set; }

    /*public bool IsActive { get { return itemsQueryTask!=null; } }*/

    public void RegisterQueryTask()
    {
      EntitySetState state;
      if (processor.Owner.TryGetEntitySetState(ownerKey, ReferencingField, out state)
        && (state.IsFullyLoaded || state.Count >= ItemCountLimit))
        return;
      itemsQueryTask = CreateQueryTask();
      processor.Owner.Session.RegisterDelayedQuery(itemsQueryTask);
    }

    public void UpdateCache()
    {
      if (itemsQueryTask == null)
        return;
      var reader = processor.Owner.Session.Domain.RecordSetReader;
      var records = reader.Read(itemsQueryTask.Result, RecordSet.Header);
      var entityKeys = new List<Key>(itemsQueryTask.Result.Count);
      List<Pair<Key, Tuple>> auxEntities = null;
      if (ReferencingField.Association.AuxiliaryType != null)
        auxEntities = new List<Pair<Key, Tuple>>(itemsQueryTask.Result.Count);
      foreach (var record in records) {
        for (int i = 0; i < record.Count; i++) {
          var key = record.GetKey(i);
          if (key==null)
            continue;
          var tuple = record.GetTuple(i);
          if (tuple==null)
            continue;
          if (ReferencingField.Association.AuxiliaryType != null)
            if (i==0)
              auxEntities.Add(new Pair<Key, Tuple>(key, tuple));
            else {
              processor.SaveStrongReference(processor.Owner.RegisterEntityState(key, tuple));
              entityKeys.Add(key);
            }
          else {
            processor.SaveStrongReference(processor.Owner.RegisterEntityState(key, tuple));
            entityKeys.Add(key);
          }
        }
      }
      processor.Owner.RegisterEntitySetState(ownerKey, ReferencingField,
        ItemCountLimit == null || entityKeys.Count < ItemCountLimit, entityKeys, auxEntities);
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

    #region Private \ internal methods

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
      var joiningColumns = GetJoiningColumnIndexes(primaryTargetIndex, associationIndex,
        pair.Second.ReferencingField.Association.AuxiliaryType != null);
      var resultColumns = new List<int>(primaryTargetIndex.Columns.Count);
      if (pair.Second.ReferencingField.Association.AuxiliaryType == null)
        AddResultColumnIndexes(resultColumns, primaryTargetIndex, associationIndex.Columns.Count);
      else {
        AddResultColumnIndexes(resultColumns, associationIndex, 0);
        AddResultColumnIndexes(resultColumns, primaryTargetIndex, resultColumns.Count);
      }
      ParameterExpression tupleParameter;
      var filterExpression = CreateFilterExpression(associationIndex, out tupleParameter);
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
        if (PrefetchHelper.IsFieldToBeLoadedByDefault(column.Field))
          indexes.Add(i + columnIndexOffset);
      }
    }

    private static Pair<int>[] GetJoiningColumnIndexes(IndexInfo primaryIndex, IndexInfo associationIndex,
      bool hasAuxType)
    {
      var joiningColumns = new Pair<int>[primaryIndex.KeyColumns.Count];
      var firstColumnIndex = primaryIndex.Columns.IndexOf(primaryIndex.KeyColumns[0].Key);
      for (int i = 0; i < joiningColumns.Length; i++)
        if (hasAuxType)
          joiningColumns[i] =
            new Pair<int>(associationIndex.Columns.IndexOf(associationIndex.ValueColumns[i]),
              firstColumnIndex + i);
        else
          joiningColumns[i] =
            new Pair<int>(associationIndex.Columns.IndexOf(primaryIndex.KeyColumns[i].Key),
              firstColumnIndex + i);
      return joiningColumns;
    }

    private static Expression CreateFilterExpression(IndexInfo associationIndex,
      out ParameterExpression tupleParameter)
    {
      Expression filterExpression = null;
      tupleParameter = Expression.Parameter(typeof (Tuple), "tuple");
      var valueProperty = typeof (Parameter<Tuple>).GetProperty("Value", typeof (Tuple));
      var keyValue = Expression.Property(Expression.Constant(ownerParameter), valueProperty);
      var firstKeyColumnIndex = associationIndex.Columns.IndexOf(associationIndex.KeyColumns[0].Key);
      for (var i = 0; i < associationIndex.KeyColumns.Count; i++) {
        var getValueMethod = getValueMethodDefinition
          .MakeGenericMethod(associationIndex.Columns[firstKeyColumnIndex + i].ValueType);
        var tupleParameterFieldAccess = Expression.Call(tupleParameter, getValueMethod,
          Expression.Constant(firstKeyColumnIndex + i));
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

    #endregion


    // Constructors

    public EntitySetPrefetchTask(Key ownerKey, PrefetchFieldDescriptor referencingFieldDescriptor,
      PrefetchProcessor processor)
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
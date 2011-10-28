// Copyright (C) 2003-2010 Xtensive LLC.
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
using Xtensive.Parameters;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Model;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Rse;
using FieldInfo=Xtensive.Orm.Model.FieldInfo;

namespace Xtensive.Orm.Internals.Prefetch
{
  [Serializable]
  internal sealed class EntitySetTask : IEquatable<EntitySetTask>
  {
    #region Nested classes

    private struct CacheKey : IEquatable<CacheKey>
    {
      public readonly FieldInfo ReferencingField;
      public readonly int? ItemCountLimit;
      private int? cachedHashCode;

      public bool Equals(CacheKey other)
      {
        return other.ItemCountLimit.Equals(ItemCountLimit) && Equals(other.ReferencingField, ReferencingField);
      }

      public override bool Equals(object obj)
      {
        if (ReferenceEquals(null, obj))
          return false;
        if (obj.GetType()!=typeof (CacheKey))
          return false;
        return Equals((CacheKey) obj);
      }

      public override int GetHashCode()
      {
        if (cachedHashCode==null)
          unchecked {
            cachedHashCode = (ReferencingField.GetHashCode() * 397)
              ^ (ItemCountLimit.HasValue ? 1 : 0);
          }
        return cachedHashCode.Value;
      }


      // Constructors

      public CacheKey(FieldInfo referencingField, int? itemCountLimit)
      {
        this.ReferencingField = referencingField;
        this.ItemCountLimit = itemCountLimit;
        cachedHashCode = null;
      }
    }

    #endregion

    private static readonly object itemsQueryCachingRegion = new object();
    private static readonly Parameter<Tuple> ownerParameter = new Parameter<Tuple>(WellKnown.KeyFieldName);
    private static readonly Parameter<int> itemCountLimitParameter = new Parameter<int>("ItemCountLimit");
    private static readonly MethodInfo getValueMethodDefinition = typeof (Tuple)
      .GetMethods(BindingFlags.Public | BindingFlags.Instance)
      .Where(method => method.Name=="GetValue" && method.GetParameters().Length == 1
        && method.IsGenericMethodDefinition).Single();

    private readonly Key ownerKey;
    private readonly bool isOwnerCached;
    private readonly PrefetchManager manager;
    private QueryTask itemsQueryTask;
    private int? cachedHashCode;
    private readonly PrefetchFieldDescriptor referencingFieldDescriptor;
    private readonly CacheKey cacheKey;

    public FieldInfo ReferencingField {get { return referencingFieldDescriptor.Field; } }

    public RecordQuery RecordQuery { get; private set; }

    public int? ItemCountLimit { get; private set; }

    public void RegisterQueryTask()
    {
      EntitySetState state;
      if (isOwnerCached && manager.Owner.TryGetEntitySetState(ownerKey, ReferencingField, out state))
        if (state==null || state.IsFullyLoaded)
          return;
      itemsQueryTask = CreateQueryTask();
      manager.Owner.Session.RegisterDelayedQuery(itemsQueryTask);
    }

    public void UpdateCache()
    {
      if (itemsQueryTask==null)
        return;
      var areToNotifyAboutKeys = !manager.Owner.Session.Domain.Model
        .Types[referencingFieldDescriptor.Field.ItemType].IsLeaf;
      var reader = manager.Owner.Session.Domain.RecordSetReader;
      var records = reader.Read(itemsQueryTask.Result, RecordQuery.Header, manager.Owner.Session);
      var entityKeys = new List<Key>(itemsQueryTask.Result.Count);
      List<Pair<Key, Tuple>> auxEntities = null;
      var association = ReferencingField.Associations.Last();
      if (association.AuxiliaryType!=null)
        auxEntities = new List<Pair<Key, Tuple>>(itemsQueryTask.Result.Count);
      foreach (var record in records) {
        for (int i = 0; i < record.Count; i++) {
          var key = record.GetKey(i);
          if (key==null)
            continue;
          var tuple = record.GetTuple(i);
          if (tuple==null)
            continue;
          if (association.AuxiliaryType!=null)
            if (i==0)
              auxEntities.Add(new Pair<Key, Tuple>(key, tuple));
            else {
              manager.SaveStrongReference(manager.Owner.RegisterEntityState(key, tuple));
              entityKeys.Add(key);
              if (areToNotifyAboutKeys)
                referencingFieldDescriptor.NotifySubscriber(ownerKey, key);
            }
          else {
            manager.SaveStrongReference(manager.Owner.RegisterEntityState(key, tuple));
            entityKeys.Add(key);
            if (areToNotifyAboutKeys)
              referencingFieldDescriptor.NotifySubscriber(ownerKey, key);
          }
        }
      }
      manager.Owner.RegisterEntitySetState(ownerKey, ReferencingField,
        ItemCountLimit==null || entityKeys.Count < ItemCountLimit, entityKeys, auxEntities);
    }

    public bool Equals(EntitySetTask other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return other.cacheKey.Equals(cacheKey);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType()!=typeof (EntitySetTask))
        return false;
      return Equals((EntitySetTask) obj);
    }

    public override int GetHashCode()
    {
      return cacheKey.GetHashCode();
    }

    #region Private \ internal methods

    private QueryTask CreateQueryTask()
    {
      var parameterContext = new ParameterContext();
      using (parameterContext.Activate()) {
        ownerParameter.Value = ownerKey.Value;
        if (ItemCountLimit != null)
          itemCountLimitParameter.Value = ItemCountLimit.Value;
        object key = new Pair<object, CacheKey>(itemsQueryCachingRegion, cacheKey);
        Func<object, object> generator = CreateRecordSetLoadingItems;
        RecordQuery = (RecordQuery) manager.Owner.Session.Domain.Cache.GetValue(key, generator);
        var executableProvider = manager.Owner.Session.CompilationService.Compile(RecordQuery.Provider);
        return new QueryTask(executableProvider, parameterContext);
      }
    }

    private static RecordQuery CreateRecordSetLoadingItems(object cachingKey)
    {
      var pair = (Pair<object, CacheKey>) cachingKey;
      var association = pair.Second.ReferencingField.Associations.Last();
      var primaryTargetIndex = association.TargetType.Indexes.PrimaryIndex;
      var resultColumns = new List<int>(primaryTargetIndex.Columns.Count);
      ParameterExpression tupleParameter;
      RecordQuery result;
      if (association.AuxiliaryType == null)
        result = CreateQueryForDirectAssociation(pair, primaryTargetIndex, resultColumns);
      else
        result = CreateQueryForAssociationViaAuxType(pair, primaryTargetIndex, resultColumns);
      result = result.Select(resultColumns.ToArray());
      if (pair.Second.ItemCountLimit != null)
        result = result.Take(() => itemCountLimitParameter.Value);
      return result;
    }

    private static RecordQuery CreateQueryForAssociationViaAuxType(Pair<object, CacheKey> pair, IndexInfo primaryTargetIndex, List<int> resultColumns)
    {
      var association = pair.Second.ReferencingField.Associations.Last();
      var associationIndex = association.UnderlyingIndex;
      var joiningColumns = GetJoiningColumnIndexes(primaryTargetIndex, associationIndex,
        association.AuxiliaryType != null);
      AddResultColumnIndexes(resultColumns, associationIndex, 0);
      AddResultColumnIndexes(resultColumns, primaryTargetIndex, resultColumns.Count);
      var firstKeyColumnIndex = associationIndex.Columns.IndexOf(associationIndex.KeyColumns[0].Key);
      var keyColumnTypes = associationIndex
        .Columns
        .Skip(firstKeyColumnIndex)
        .Take(associationIndex.KeyColumns.Count)
        .Select(column => column.ValueType)
        .ToList();
      return associationIndex.ToRecordQuery()
        .Filter(QueryHelper.BuildFilterLambda(firstKeyColumnIndex, keyColumnTypes, ownerParameter))
        .Alias("a")
        .Join(primaryTargetIndex.ToRecordQuery(), joiningColumns);
    }

    private static RecordQuery CreateQueryForDirectAssociation(Pair<object, CacheKey> pair, IndexInfo primaryTargetIndex, List<int> resultColumns)
    {
      AddResultColumnIndexes(resultColumns, primaryTargetIndex, 0);
      var association = pair.Second.ReferencingField.Associations.Last();
      var field = association.Reversed.OwnerField;
      var keyColumnTypes = field.Columns.Select(column => column.ValueType).ToList();
      return primaryTargetIndex
        .ToRecordQuery()
        .Filter(QueryHelper.BuildFilterLambda(field.MappingInfo.Offset, keyColumnTypes, ownerParameter));
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

    private static Pair<int>[] GetJoiningColumnIndexes(IndexInfo primaryIndex, IndexInfo associationIndex, bool hasAuxType)
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

    #endregion


    // Constructors

    public EntitySetTask(Key ownerKey, PrefetchFieldDescriptor referencingFieldDescriptor, bool isOwnerCached,
      PrefetchManager manager)
    {
      ArgumentValidator.EnsureArgumentNotNull(ownerKey, "ownerKey");
      ArgumentValidator.EnsureArgumentNotNull(referencingFieldDescriptor, "referencingFieldDescriptor");
      ArgumentValidator.EnsureArgumentNotNull(manager, "processor");

      this.ownerKey = ownerKey;
      this.referencingFieldDescriptor = referencingFieldDescriptor;
      this.isOwnerCached = isOwnerCached;
      ItemCountLimit = referencingFieldDescriptor.EntitySetItemCountLimit;
      this.manager = manager;
      cacheKey = new CacheKey(ReferencingField, ItemCountLimit);
    }
  }
}
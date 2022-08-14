// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexander Nikolaev
// Created:    2009.09.09

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Model;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Providers;
using FieldInfo = Xtensive.Orm.Model.FieldInfo;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Internals.Prefetch
{
  internal readonly struct ItemsQueryCacheKey : IEquatable<ItemsQueryCacheKey>
  {
    public readonly FieldInfo ReferencingField;
    public readonly int? ItemCountLimit;
    private readonly int cachedHashCode;

    public bool Equals(ItemsQueryCacheKey other)
    {
      return (ItemCountLimit == null) == (other.ItemCountLimit == null)
        && Equals(other.ReferencingField, ReferencingField);
    }

    public override bool Equals(object obj) =>
      obj is ItemsQueryCacheKey other && Equals(other);

    public override int GetHashCode() => cachedHashCode;

    // Constructors

    public ItemsQueryCacheKey(FieldInfo referencingField, int? itemCountLimit)
    {
      ReferencingField = referencingField;
      ItemCountLimit = itemCountLimit;
      unchecked {
        cachedHashCode = (ReferencingField.GetHashCode() * 397)
                         ^ (ItemCountLimit.HasValue ? 1 : 0);
      }
    }
  }

  [Serializable]
  internal sealed class EntitySetTask : IEquatable<EntitySetTask>
  {
    private static readonly Parameter<Tuple> ownerParameter = new Parameter<Tuple>(WellKnown.KeyFieldName);
    private static readonly Parameter<int> itemCountLimitParameter = new Parameter<int>("ItemCountLimit");

    private static readonly Func<ItemsQueryCacheKey, CompilableProvider> CreateRecordSetLoadingItems = cachingKey => {
      var association = cachingKey.ReferencingField.Associations.Last();
      var primaryTargetIndex = association.TargetType.Indexes.PrimaryIndex;
      var resultColumns = new List<int>(primaryTargetIndex.Columns.Count);
      var result = association.AuxiliaryType == null
        ? CreateQueryForDirectAssociation(cachingKey, primaryTargetIndex, resultColumns)
        : CreateQueryForAssociationViaAuxType(cachingKey, primaryTargetIndex, resultColumns);
      result = result.Select(resultColumns);
      if (cachingKey.ItemCountLimit != null)
        result = result.Take(context => context.GetValue(itemCountLimitParameter));
      return result;
    };

    private readonly Key ownerKey;
    private readonly bool isOwnerCached;
    private readonly PrefetchManager manager;
    private QueryTask itemsQueryTask;
    private readonly PrefetchFieldDescriptor referencingFieldDescriptor;
    private readonly ItemsQueryCacheKey cacheKey;

    public FieldInfo ReferencingField {get { return referencingFieldDescriptor.Field; } }

    public CompilableProvider QueryProvider { get; private set; }

    public int? ItemCountLimit { get; private set; }

    public void RegisterQueryTask()
    {
      if (isOwnerCached && manager.Owner.LookupState(ownerKey, ReferencingField, out var state)) {
        if (state == null || (state.IsFullyLoaded && !state.ShouldUseForcePrefetch(referencingFieldDescriptor.PrefetchOperationId))) {
          return;
        }
      }

      itemsQueryTask = CreateQueryTask();
      manager.Owner.Session.RegisterInternalDelayedQuery(itemsQueryTask);
    }

    public void UpdateCache()
    {
      if (itemsQueryTask == null) {
        return;
      }

      var areToNotifyAboutKeys = !manager.Owner.Session.Domain.Model
        .Types[referencingFieldDescriptor.Field.ItemType].IsLeaf;
      var reader = manager.Owner.Session.Domain.EntityDataReader;
      var records = reader.Read(itemsQueryTask.Result, QueryProvider.Header, manager.Owner.Session);
      var entityKeys = new List<Key>(itemsQueryTask.Result.Count);
      var association = ReferencingField.Associations.Last();
      var auxEntities = (association.AuxiliaryType != null)
        ? new List<Pair<Key, Tuple>>(itemsQueryTask.Result.Count)
        : null;

      foreach (var record in records) {
        for (var i = 0; i < record.Count; i++) {
          var key = record.GetKey(i);
          if (key == null) {
            continue;
          }
          var tuple = record.GetTuple(i);
          if (tuple == null) {
            continue;
          }
          if (association.AuxiliaryType != null) {
            if (i == 0) {
              auxEntities.Add(new Pair<Key, Tuple>(key, tuple));
            }
            else {
              manager.SaveStrongReference(manager.Owner.UpdateState(key, tuple));
              entityKeys.Add(key);
              if (areToNotifyAboutKeys) {
                referencingFieldDescriptor.NotifySubscriber(ownerKey, key);
              }
            }
          }
          else {
            manager.SaveStrongReference(manager.Owner.UpdateState(key, tuple));
            entityKeys.Add(key);
            if (areToNotifyAboutKeys) {
              referencingFieldDescriptor.NotifySubscriber(ownerKey, key);
            }
          }
        }
      }
      var updatedState = manager.Owner.UpdateState(ownerKey, ReferencingField,
        ItemCountLimit == null || entityKeys.Count < ItemCountLimit, entityKeys, auxEntities);
      if (updatedState != null) {
        updatedState.SetLastManualPrefetchId(referencingFieldDescriptor.PrefetchOperationId);
      }
    }

    public bool Equals(EntitySetTask other)
    {
      if (other is null) {
        return false;
      }
      if (ReferenceEquals(this, other)) {
        return true;
      }
      return other.cacheKey.Equals(cacheKey);
    }

    public override bool Equals(object obj) =>
      ReferenceEquals(this, obj)
        || obj is EntitySetTask other && Equals(other);

    public override int GetHashCode() => cacheKey.GetHashCode();

    #region Private / internal methods

    private QueryTask CreateQueryTask()
    {
      var parameterContext = new ParameterContext();
      parameterContext.SetValue(ownerParameter, ownerKey.Value);
      if (ItemCountLimit != null) {
        parameterContext.SetValue(itemCountLimitParameter, ItemCountLimit.Value);
      }

      var session = manager.Owner.Session;
      var scope = new CompiledQueryProcessingScope(null, null, parameterContext, false);
      QueryProvider = session.StorageNode.EntitySetFetchQueryCache.GetOrAdd(cacheKey, CreateRecordSetLoadingItems);
      if (session.Domain.TagsEnabled && session.Tags != null) {
        foreach (var tag in session.Tags) {
          QueryProvider = new TagProvider(QueryProvider, tag);
        }
      }
      ExecutableProvider executableProvider;
      using (scope.Enter()) {
        executableProvider = session.Compile(QueryProvider);
      }
      return new QueryTask(executableProvider, session.GetLifetimeToken(), parameterContext);
    }

    private static CompilableProvider CreateQueryForAssociationViaAuxType(in ItemsQueryCacheKey cachingKey, IndexInfo primaryTargetIndex, List<int> resultColumns)
    {
      var association = cachingKey.ReferencingField.Associations.Last();
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
      return associationIndex.GetQuery()
        .Filter(QueryHelper.BuildFilterLambda(firstKeyColumnIndex, keyColumnTypes, ownerParameter))
        .Alias("a")
        .Join(primaryTargetIndex.GetQuery(), joiningColumns);
    }

    private static CompilableProvider CreateQueryForDirectAssociation(in ItemsQueryCacheKey cachingKey, IndexInfo primaryTargetIndex, List<int> resultColumns)
    {
      AddResultColumnIndexes(resultColumns, primaryTargetIndex, 0);
      var association = cachingKey.ReferencingField.Associations.Last();
      var field = association.Reversed.OwnerField;
      var keyColumnTypes = field.Columns.Select(column => column.ValueType).ToList();
      return primaryTargetIndex
        .GetQuery()
        .Filter(QueryHelper.BuildFilterLambda(field.MappingInfo.Offset, keyColumnTypes, ownerParameter));
    }

    private static void AddResultColumnIndexes(ICollection<int> indexes, IndexInfo index,
      int columnIndexOffset)
    {
      for (var i = 0; i < index.Columns.Count; i++) {
        var column = index.Columns[i];
        if (PrefetchHelper.IsFieldToBeLoadedByDefault(column.Field)) {
          indexes.Add(i + columnIndexOffset);
        }
      }
    }

    private static Pair<int>[] GetJoiningColumnIndexes(IndexInfo primaryIndex, IndexInfo associationIndex, bool hasAuxType)
    {
      var joiningColumns = new Pair<int>[primaryIndex.KeyColumns.Count];
      var firstColumnIndex = primaryIndex.Columns.IndexOf(primaryIndex.KeyColumns[0].Key);
      for (var i = 0; i < joiningColumns.Length; i++) {
        if (hasAuxType) {
          joiningColumns[i] =
            new Pair<int>(associationIndex.Columns.IndexOf(associationIndex.ValueColumns[i]),
              firstColumnIndex + i);
        }
        else {
          joiningColumns[i] =
            new Pair<int>(associationIndex.Columns.IndexOf(primaryIndex.KeyColumns[i].Key),
              firstColumnIndex + i);
        }
      }

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
      cacheKey = new ItemsQueryCacheKey(ReferencingField, ItemCountLimit);
    }
  }
}

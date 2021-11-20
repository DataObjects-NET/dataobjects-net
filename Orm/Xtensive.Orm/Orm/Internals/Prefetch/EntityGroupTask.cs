// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexander Nikolaev
// Created:    2009.10.20

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Providers;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Internals.Prefetch
{
  internal readonly struct RecordSetCacheKey : IEquatable<RecordSetCacheKey>
  {
    public readonly int[] ColumnIndexes;
    public readonly TypeInfo Type;
    private readonly int cachedHashCode;

    public bool Equals(RecordSetCacheKey other)
    {
      if (!Type.Equals(other.Type)) {
        return false;
      }

      if (ColumnIndexes.Length != other.ColumnIndexes.Length) {
        return false;
      }

      for (var i = ColumnIndexes.Length - 1; i >= 0; i--) {
        if (ColumnIndexes[i] != other.ColumnIndexes[i]) {
          return false;
        }
      }

      return true;
    }

    public override bool Equals(object obj) =>
      obj is RecordSetCacheKey other && Equals(other);

    public override int GetHashCode() => cachedHashCode;


    // Constructors

    public RecordSetCacheKey(int[] columnIndexes, TypeInfo type, int cachedHashCode)
    {
      ColumnIndexes = columnIndexes;
      Type = type;
      this.cachedHashCode = cachedHashCode;
    }
  }

  [Serializable]
  internal sealed class EntityGroupTask : IEquatable<EntityGroupTask>
  {
    private const int MaxKeyCountInOneStatement = 40;
    private static readonly Parameter<IEnumerable<Tuple>> includeParameter =
      new Parameter<IEnumerable<Tuple>>("Keys");

    private static readonly Func<RecordSetCacheKey, CompilableProvider> CreateRecordSet = cachingKey => {
      var selectedColumnIndexes = cachingKey.ColumnIndexes;
      var keyColumnsCount = cachingKey.Type.Indexes.PrimaryIndex.KeyColumns.Count;
      var keyColumnIndexes = new int[keyColumnsCount];
      foreach (var index in Enumerable.Range(0, keyColumnsCount)) {
        keyColumnIndexes[index] = index;
      }

      var columnCollectionLength = cachingKey.Type.Indexes.PrimaryIndex.Columns.Count;
      return cachingKey.Type.Indexes.PrimaryIndex.GetQuery().Include(IncludeAlgorithm.ComplexCondition,
        true, context => context.GetValue(includeParameter), $"includeColumnName-{Guid.NewGuid()}",
        keyColumnIndexes).Filter(t => t.GetValue<bool>(columnCollectionLength)).Select(selectedColumnIndexes);
    };

    private Dictionary<Key, bool> keys;
    private readonly TypeInfo type;
    private readonly PrefetchManager manager;
    private List<QueryTask> queryTasks;
    private readonly RecordSetCacheKey cacheKey;

    public CompilableProvider Provider { get; private set; }

    public void AddKey(Key key, bool exactType) =>
      (keys ??= new Dictionary<Key, bool>()).TryAdd(key, exactType);

    public void RegisterQueryTasks()
    {
      queryTasks = new List<QueryTask>(keys.Count);
      var count = 0;
      var keyCount = keys.Count;
      var totalCount = 0;
      List<Tuple> currentKeySet = null;
      foreach (var pair in keys) {
        if (count == 0) {
          currentKeySet = new List<Tuple>(MaxKeyCountInOneStatement);
        }

        currentKeySet.Add(pair.Key.Value);
        count++;
        totalCount++;
        if (count == MaxKeyCountInOneStatement || totalCount == keyCount) {
          count = 0;
          var queryTask = CreateQueryTask(currentKeySet);
          queryTasks.Add(queryTask);
          manager.Owner.Session.RegisterInternalDelayedQuery(queryTask);
        }
      }
    }

    public void UpdateCache(HashSet<Key> foundKeys)
    {
      var reader = manager.Owner.Session.Domain.EntityDataReader;
      foreach (var queryTask in queryTasks) {
        PutLoadedStatesInCache(queryTask.Result, reader, foundKeys);
      }

      HandleMissedKeys(foundKeys);
    }

    public bool Equals(EntityGroupTask other)
    {
      if (ReferenceEquals(null, other)) {
        return false;
      }

      return ReferenceEquals(this, other) || other.cacheKey.Equals(cacheKey);
    }

    public override bool Equals(object obj) =>
      ReferenceEquals(this, obj)
        || obj is EntityGroupTask other && Equals(other);

    public override int GetHashCode() => cacheKey.GetHashCode();

    private QueryTask CreateQueryTask(List<Tuple> currentKeySet)
    {
      var parameterContext = new ParameterContext();
      parameterContext.SetValue(includeParameter, currentKeySet);
      var session = manager.Owner.Session;
      Provider = session.StorageNode.InternalRecordSetCache.GetOrAdd(cacheKey, CreateRecordSet);
      var executableProvider = session.Compile(Provider);
      return new QueryTask(executableProvider, session.GetLifetimeToken(), parameterContext);
    }

    private void PutLoadedStatesInCache(IEnumerable<Tuple> queryResult, EntityDataReader reader,
      HashSet<Key> foundedKeys)
    {
      var entityRecords = reader.Read(queryResult, Provider.Header, manager.Owner.Session);
      foreach (var entityRecord in entityRecords) {
        var fetchedKey = entityRecord.GetKey();
        var tuple = entityRecord.GetTuple();
        if (tuple != null) {
          manager.SaveStrongReference(manager.Owner.UpdateState(fetchedKey, tuple));
          foundedKeys.Add(fetchedKey);
        }
      }
    }

    private void HandleMissedKeys(HashSet<Key> foundKeys)
    {
      if (foundKeys.Count == keys.Count) {
        return;
      }

      var countOfHandledKeys = foundKeys.Count;
      var totalCount = keys.Count;
      foreach (var pair in keys) {
        if (!foundKeys.Contains(pair.Key)) {
          MarkMissedEntityState(pair.Key, pair.Value);
          countOfHandledKeys++;
        }

        if (countOfHandledKeys == totalCount) {
          break;
        }
      }
    }

    private void MarkMissedEntityState(Key key, bool exactType)
    {
      var cachedEntityState = manager.GetCachedEntityState(ref key, out var isRemoved);
      if (exactType && !isRemoved
        && (cachedEntityState==null || cachedEntityState.Key.HasExactType && cachedEntityState.Key
          .TypeReference.Type==type)) {
        // Ensures there will be "removed" EntityState associated with this key
        manager.SaveStrongReference(manager.Owner.UpdateState(key, null));
      }
    }


    // Constructors

    public EntityGroupTask(TypeInfo type, int[] columnIndexes, PrefetchManager manager)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, nameof(type));
      ArgumentValidator.EnsureArgumentNotNull(columnIndexes, nameof(columnIndexes));
      ArgumentValidator.EnsureArgumentIsGreaterThan(columnIndexes.Length, 0, "columnIndexes.Length");
      ArgumentValidator.EnsureArgumentNotNull(manager, nameof(manager));

      this.type = type;
      this.manager = manager;
      var cachedHashCode = 0;
      foreach (var columnIndex in columnIndexes) {
        cachedHashCode = unchecked (379 * cachedHashCode + columnIndex);
      }

      cachedHashCode ^= type.GetHashCode();
      cacheKey = new RecordSetCacheKey(columnIndexes, type, cachedHashCode);
    }
  }
}

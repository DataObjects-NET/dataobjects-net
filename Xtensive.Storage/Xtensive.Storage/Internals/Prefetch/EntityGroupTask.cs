// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.10.20

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Internals.Prefetch
{
  [Serializable]
  internal sealed class EntityGroupTask : IEquatable<EntityGroupTask>
  {
    #region Nested classes

    private struct CacheKey : IEquatable<CacheKey>
    {
      private readonly int[] columnIndexes;
      private readonly TypeInfo type;
      private readonly int cachedHashCode;

      public bool Equals(CacheKey other)
      {
        if (!type.Equals(other.type))
          return false;
        if (columnIndexes.Length!=other.columnIndexes.Length)
          return false;
        for (var i = columnIndexes.Length - 1; i >= 0; i--)
          if (columnIndexes[i]!=other.columnIndexes[i])
            return false;
        return true;
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
        return cachedHashCode;
      }


      // Constructors

      public CacheKey(int[] columnIndexes, TypeInfo type, int cachedHashCode)
      {
        this.columnIndexes = columnIndexes;
        this.type = type;
        this.cachedHashCode = cachedHashCode;
      }
    }

    #endregion

    private const int MaxKeyCountInOneStatement = 40;
    private static readonly object recordSetCachingRegion = new object();
    private static readonly Parameter<Tuple> seekParameter = new Parameter<Tuple>(WellKnown.KeyFieldName);
    private static readonly Parameter<IEnumerable<Tuple>> includeParameter =
      new Parameter<IEnumerable<Tuple>>("Keys");
    private static readonly IEqualityComparer<HashSet<int>> columnIndexesComparer =
      HashSet<int>.CreateSetComparer();

    private readonly int[] columnIndexes;
    private Dictionary<Key, bool> keys;
    private readonly TypeInfo type;
    private readonly int cachedHashCode;
    private readonly PrefetchManager manager;
    private List<QueryTask> queryTasks;
    private readonly CacheKey cacheKey;

    public RecordSet RecordSet { get; private set; }

    public void AddKey(Key key, bool exactType)
    {
      if (keys == null)
        keys = new Dictionary<Key, bool>();
      if (keys.ContainsKey(key))
        return;
      keys.Add(key, exactType);
    }

    public void RegisterQueryTasks()
    {
      queryTasks = new List<QueryTask>(keys.Count);
      var skipCount = 0;
      var count = 0;
      var keyCount = keys.Count;
      var totalCount = 0;
      List<Tuple> currentKeySet = null;
      foreach (var pair in keys) {
        if (count == 0)
          currentKeySet = new List<Tuple>(MaxKeyCountInOneStatement);
        currentKeySet.Add(pair.Key.Value);
        count++;
        totalCount++;
        if (count == MaxKeyCountInOneStatement || totalCount == keyCount) {
          count = 0;
          var queryTask = CreateQueryTask(currentKeySet);
          queryTasks.Add(queryTask);
          manager.Owner.Session.RegisterDelayedQuery(queryTask);
        }
      }
    }

    public void UpdateCache(HashSet<Key> foundKeys)
    {
      var reader = manager.Owner.Session.Domain.RecordSetReader;
      foreach (var queryTask in queryTasks)
        PutLoadedStatesInCache(queryTask.Result, reader, foundKeys);
      HandleMissedKeys(foundKeys);
    }

    public bool Equals(EntityGroupTask other)
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
      if (obj.GetType()!=typeof (EntityGroupTask))
        return false;
      return Equals((EntityGroupTask) obj);
    }

    public override int GetHashCode()
    {
      return cacheKey.GetHashCode();
    }

    private QueryTask CreateQueryTask(List<Tuple> currentKeySet)
    {
      var parameterContext = new ParameterContext();
      using (parameterContext.Activate()) {
        includeParameter.Value = currentKeySet;
        object key = new Pair<object, CacheKey>(recordSetCachingRegion, cacheKey);
        Func<object, object> generator = CreateRecordSet;
        RecordSet = (RecordSet) manager.Owner.Session.Domain.Cache.GetValue(key, generator);
        var executableProvider = CompilationContext.Current.Compile(RecordSet.Provider);
        return new QueryTask(executableProvider, parameterContext);
      }
    }

    private static RecordSet CreateRecordSet(object cachingKey)
    {
      var pair = (Pair<object, EntityGroupTask>) cachingKey;
      var selectedColumnIndexes = pair.Second.columnIndexes;
      var keyColumnIndexes = EnumerableUtils.Unfold(0, i => i + 1)
        .Take(pair.Second.type.Indexes.PrimaryIndex.KeyColumns.Count).ToArray();
      var columnCollectionLenght = pair.Second.type.Indexes.PrimaryIndex.Columns.Count;
      return pair.Second.type.Indexes.PrimaryIndex.ToRecordSet().Include(IncludeAlgorithm.ComplexCondition,
        true, () => includeParameter.Value, String.Format("includeColumnName-{0}", Guid.NewGuid()),
        keyColumnIndexes).Filter(t => t.GetValue<bool>(columnCollectionLenght)).Select(selectedColumnIndexes);
    }

    private void PutLoadedStatesInCache(IEnumerable<Tuple> queryResult, RecordSetReader reader,
      HashSet<Key> foundedKeys)
    {
      var records = reader.Read(queryResult, RecordSet.Header);
      foreach (var record in records) {
        if (record!=null) {
          var fetchedKey = record.GetKey();
          var tuple = record.GetTuple();
          if (tuple!=null) {
            manager.SaveStrongReference(manager.Owner.RegisterEntityState(fetchedKey, tuple));
            foundedKeys.Add(fetchedKey);
          }
        }
      }
    }

    private void HandleMissedKeys(HashSet<Key> foundKeys)
    {
      if (foundKeys.Count == keys.Count)
        return;
      var countOfHandledKeys = foundKeys.Count;
      var totalCount = keys.Count;
      foreach (var pair in keys) {
        if (!foundKeys.Contains(pair.Key)) {
          MarkMissedEntityState(pair.Key, pair.Value);
          countOfHandledKeys++;
        }
        if (countOfHandledKeys == totalCount)
          break;
      }
    }

    private void MarkMissedEntityState(Key key, bool exactType)
    {
      bool isRemoved;
      var cachedEntityState = manager.GetCachedEntityState(ref key, out isRemoved);
      if (exactType && !isRemoved
        && (cachedEntityState==null || cachedEntityState.Key.HasExactType && cachedEntityState.Key
          .TypeReference.Type==type))
        // Ensures there will be "removed" EntityState associated with this key
        manager.SaveStrongReference(manager.Owner.RegisterEntityState(key, null));
    }


    // Constructors

    public EntityGroupTask(TypeInfo type, int[] columnIndexes, PrefetchManager manager)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      ArgumentValidator.EnsureArgumentNotNull(columnIndexes, "columnIndexes");
      ArgumentValidator.EnsureArgumentIsGreaterThan(columnIndexes.Length, 0, "columnIndexes.Length");
      ArgumentValidator.EnsureArgumentNotNull(manager, "processor");

      this.type = type;
      this.columnIndexes = columnIndexes;
      this.manager = manager;
      var cachedHashCode = 0;
      for (var i = 0; i < columnIndexes.Length; i++)
        cachedHashCode = unchecked (379 * cachedHashCode + columnIndexes[i]);
      cachedHashCode = unchecked (cachedHashCode ^ type.GetHashCode());
      cacheKey = new CacheKey(columnIndexes, type, cachedHashCode);
    }
  }
}
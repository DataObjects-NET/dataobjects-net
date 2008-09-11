// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.10

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Integrity.Transactions;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Internals
{
  internal class SimpleEntitySet<T> : EntitySet<T>
    where T : Entity
  {
    #region Nested types

    private class CachedKey : IIdentified<Key>,
      IHasSize
    {
      public Key Key { get; private set; }

      object IIdentified.Identifier
      {
        get { return Identifier; }
      }

      public Key Identifier
      {
        get { return Key; }
      }

      public long Size
      {
        get { return 1; }
      }

      public CachedKey(Key key)
      {
        Key = key;
      }
    }

    #endregion

    private readonly IndexInfo index;
    private long? count;
    private Transaction transaction;
    private readonly Cache<Key, CachedKey, CachedKey> cache = new Cache<Key, CachedKey, CachedKey>(MaximumCacheSize, cachedKey => cachedKey.Key);
    private long version;
    private readonly MapTransform keyTransform;

    /// <inheritdoc/>
    public override int RemoveWhere(Predicate<T> match)
    {
      EnsureInitialized();
      var itemsToRemove = new List<T>();
      foreach (T item in this) {
        if (match(item))
          itemsToRemove.Add(item);
      }
      foreach (T itemToRemove in itemsToRemove)
        RemoveInternal(itemToRemove);
      return itemsToRemove.Count;
    }

    /// <inheritdoc/>
    public override void Clear()
    {
      EnsureInitialized();
      foreach (T itemToRemove in this.ToList()) {
        RemoveInternal(itemToRemove);
      }
    }

    /// <inheritdoc/>
    public override bool Contains(Key key)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      EnsureInitialized();
      if (key.Type.UnderlyingType!=typeof (T)) {
        return false;
      }
      if (!cache.Contains(key)) {
        // Request from database
        Tuple filterTuple = new CombineTransform(true, ((Entity) Owner).Key.Tuple.Descriptor, key.Tuple.Descriptor).Apply(TupleTransformType.Tuple, ((Entity) Owner).Key.Tuple, key.Tuple);
        if (GetRecordSet().Range(filterTuple, filterTuple).Count() > 0) {
          cache.Add(new CachedKey(key));
        }
        else {
          return false;
        }
      }
      return true;
    }

    /// <inheritdoc/>
    public override bool Contains(T item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      EnsureInitialized();
      if (cache.Contains(item.Key))
        return true;
      FieldInfo referencingField = Field.Association.PairTo.ReferencingField;
      var accessor = referencingField.GetAccessor<Entity>();
      bool result = ReferenceEquals(accessor.GetValue(item, referencingField), Owner);
      if (result)
        cache.Add(new CachedKey(item.Key));
      return result;
    }

    /// <inheritdoc/>
    public override bool Add(T item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      EnsureInitialized();
      FieldInfo referencingField = Field.Association.PairTo.ReferencingField;
      var accessor = referencingField.GetAccessor<Entity>();
      long previouseCount = count.Value;
      accessor.SetValue(item, referencingField, (Entity) Owner);
      return previouseCount!=count;
    }

    public override IEnumerator<T> GetEnumerator()
    {
      EnsureInitialized();
      long startVersion = version;
      if (count==cache.Count) {
        foreach (CachedKey key in cache) {
          EnsureVersion(startVersion);
          yield return key.Key.Resolve<T>();
        }
      }
      else {
        RecordSet recordSet = GetRecordSet();
        foreach (Tuple tuple in recordSet) {
          EnsureVersion(startVersion);
          Tuple keyTuple = keyTransform.Apply(TupleTransformType.Tuple, tuple);
          var key = Key.Get(typeof (T), keyTuple);
          yield return key.Resolve<T>();
        }
      }
    }

    /// <inheritdoc/>
    public override bool Remove(T item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      EnsureInitialized();
      if (!Contains(item))
        return false;
      RemoveInternal(item);
      return true;
    }

    /// <inheritdoc/>
    public override long Count
    {
      get
      {
        EnsureInitialized();
        return count.Value;
      }
    }

    private RecordSet GetRecordSet()
    {
      Tuple key = ((Entity) Owner).Key.Tuple;
      var rs = index.ToRecordSet().Range(key, key);
      return rs;
    }

    private void EnsureVersion(long currentVersion)
    {
      if (version!=currentVersion)
        Exceptions.CollectionHasBeenChanged(null);
    }

    private void EnsureInitialized()
    {
      EnusreTransaction();
      if (!count.HasValue) {
        count = GetRecordSet().Count();
      }
    }

    private void EnusreTransaction()
    {
      if (Transaction.Current!=transaction) {
        ClearCache();
        transaction = Transaction.Current;
        IncreaseVersion();
      }
      if (transaction==null || Transaction.Current.State!=TransactionState.Active)
        throw new InvalidOperationException(Strings.ExEntitySetInvalidBecauseTransactionIsNotActive);
    }

    private void RemoveInternal(T item)
    {
      FieldInfo referencingField = Field.Association.PairTo.ReferencingField;
      var accessor = referencingField.GetAccessor<Entity>();
      accessor.SetValue(item, referencingField, null);
    }

    internal override void ClearCache()
    {
      count = null;
      cache.Clear();
      IncreaseVersion();
    }

    private void IncreaseVersion()
    {
      unchecked {
        Interlocked.Increment(ref version);
      }
    }

    internal override void AddToCache(Key key)
    {
      EnsureInitialized();
      count++;
      cache.Add(new CachedKey(key));
      IncreaseVersion();
    }

    internal override void RemoveFromCache(Key key)
    {
      EnsureInitialized();
      count--;
      cache.Remove(key);
      IncreaseVersion();
    }

    public SimpleEntitySet(Persistent owner, FieldInfo field)
      : base(owner, field)
    {
      EnusreTransaction();
      FieldInfo referencingField = field.Association.PairTo.ReferencingField;
      index = referencingField.ReflectedType.Indexes.GetIndex(referencingField.Name);
      var keyTupleDescriptor = Owner.Session.Domain.Model.Types[typeof (T)].Hierarchy.KeyTupleDescriptor;
      IEnumerable<int> columnIndexes = index.Columns.Where(columnInfo => columnInfo.IsPrimaryKey).Select(columnInfo => index.Columns.IndexOf(columnInfo));
      keyTransform = new MapTransform(true, keyTupleDescriptor, columnIndexes.ToArray());
    }
  }
}
// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.10

using System;
using System.Collections.Generic;
using System.Linq;
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

    /// <inheritdoc/>
    public override int RemoveWhere(Predicate<T> match)
    {
      EnsureInitialized();
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public override void Clear()
    {
      EnsureInitialized();
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public override void CopyTo(T[] array, int arrayIndex)
    {
      EnsureInitialized();
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public override T this[T item]
    {
      get
      {
        EnsureInitialized();
        throw new NotImplementedException();
      }
    }

    /// <inheritdoc/>
    public override bool Contains(T item)
    {
      EnsureInitialized();
      if (!cache.Contains(item.Key)) {
        // Request from database
        Tuple filterTuple = new CombineTransform(true, ((Entity)Owner).Key.Tuple.Descriptor, item.Key.Tuple.Descriptor).Apply(TupleTransformType.Tuple, ((Entity)Owner).Key.Tuple, item.Key.Tuple);
        if (RecordSet.Range(filterTuple, filterTuple).Count() > 0) {
          cache.Add(new CachedKey(item.Key));
        }
        else {
          return false;
        }
      }
      return true;
    }

    /// <inheritdoc/>
    public override bool Add(T item)
    {
      EnsureInitialized();
      throw new NotImplementedException();
    }

    public override IEnumerator<T> GetEnumerator()
    {
      EnsureInitialized();
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public override bool Remove(T item)
    {
      EnsureInitialized();
      throw new NotImplementedException();
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

    private RecordSet RecordSet
    {
      get
      {
        Tuple key = ((Entity) Owner).Key.Tuple;
        var rs = index.ToRecordSet().Range(key, key);
        return rs;
      }
    }

    private void EnsureInitialized()
    {
      EnusreTransaction();
      if (!count.HasValue) {
        count = RecordSet.Count();
      }
    }

    private void EnusreTransaction()
    {
      if (Transaction.Current!=transaction) {
        ClearCache();
        transaction = Transaction.Current;
      }
      if (transaction==null || Transaction.Current.State!=TransactionState.Active)
        throw new InvalidOperationException(Strings.ExEntitySetInvalidBecauseTransactionIsNotActive);
    }

    internal override void ClearCache()
    {
      count = null;
      cache.Clear();
    }

    internal override void AddToCache(Key key)
    {
      EnsureInitialized();
      count++;
      cache.Add(new CachedKey(key));
    }

    internal override void RemoveFromCache(Key key)
    {
      EnsureInitialized();
      count--;
      cache.Remove(key);
    }

    public SimpleEntitySet(Persistent owner, FieldInfo field)
      : base(owner, field)
    {
      EnusreTransaction();
      FieldInfo referencingField = field.Association.PairTo.ReferencingField;
      index = referencingField.ReflectedType.Indexes.GetIndex(referencingField.Name);
    }
  }
}
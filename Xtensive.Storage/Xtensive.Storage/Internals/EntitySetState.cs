// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.14

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Caching;
using Xtensive.Integrity.Transactions;

namespace Xtensive.Storage.Internals
{
  [Serializable]
  public sealed class EntitySetState : IEnumerable<Key>,
    ITransactionalState,
    IHasVersion<long>
  {
    private const int CacheSize = 10240;
    private readonly ICache<Key, CachedKey> cache;
    private long count;
    private int version;
    private readonly Func<long> getCount;

    /// <inheritdoc/>
    public Transaction Transaction { get; private set; }

    public long Count
    {
      get { return count; }
    }

    public void Add(Key key)
    {
      Cache(key);
      count++;
      version++;
    }

    public void Cache(Key key)
    {
      cache.Add(new CachedKey(key));
    }

    public void Remove(Key key)
    {
      cache.RemoveKey(key);
      count--;
      version++;
    }

    public void Clear()
    {
      cache.Clear();
      count = 0;
      version++;
    }

    public bool Contains(Key key)
    {
      return cache.ContainsKey(key);
    }

    public bool IsConsistent
    {
      get { return count==cache.Count; }
    }

    public IEnumerator<Key> GetEnumerator()
    {
      foreach (CachedKey cachedKey in cache)
        yield return cachedKey.Key;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public void EnsureConsistency(Transaction transaction)
    {
      if (!Transaction.State.IsActive())
        Reset(transaction);
    }

    public void Reset(Transaction transaction)
    {
      Clear();
      this.Transaction = transaction;
      count = getCount();
      version++;
    }

    /// <inheritdoc/>
    object IHasVersion.Version
    {
      get { return Version; }
    }

    /// <inheritdoc/>
    public long Version
    {
      get { return version; }
    }


    // Constructor

    public EntitySetState(Func<long> getCount, Transaction transaction)
    {
      ArgumentValidator.EnsureArgumentNotNull(transaction, "transaction");

      cache = new LruCache<Key, CachedKey>(CacheSize, cachedKey => cachedKey.Key);
      this.getCount = getCount;
      Transaction = transaction;
    }
  }
}
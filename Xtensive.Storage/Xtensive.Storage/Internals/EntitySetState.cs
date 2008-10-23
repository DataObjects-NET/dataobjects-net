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
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Internals
{
  [Serializable]
  public sealed class EntitySetState : IEnumerable<Key>,
    ITransactionalState,
    IHasVersion<long>
  {
    private const int CacheSize = 10240;
    private readonly ICache<Key, CachedKey> cache;
    private Transaction transaction;
    private int count;
    private int version;
    private readonly Func<int> getCount;

    public int Count
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

    public Transaction Transaction
    {
      get { return transaction; }
    }

    public void EnsureConsistency(Transaction current)
    {
      if (current==null || current.State!=TransactionState.Active)
        throw new InvalidOperationException(Strings.ExEntitySetInvalidBecauseTransactionIsNotActive);

      if (transaction!=current)
        Reset(current);
    }

    public void Reset(Transaction current)
    {
      Clear();
      transaction = current;
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

    public EntitySetState(Func<int> getCount)
    {
      cache = new LruCache<Key, CachedKey>(CacheSize, cachedKey => cachedKey.Key);
      this.getCount = getCount;
    }
  }
}
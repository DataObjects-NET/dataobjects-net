// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.14

using System.Collections;
using System.Collections.Generic;
using Xtensive.Core.Caching;

namespace Xtensive.Storage.Internals
{
  public sealed class EntitySetState : IEnumerable<Key>,
    ITransactionalState
  {
    private const int CacheSize = 10240;
    private readonly ICache<Key, CachedKey> cache;
    private Transaction transaction;

    public int Count
    {
      get { return cache.Count; }
    }

    public void Add(Key key)
    {
      cache.Add(new CachedKey(key));
    }

    public void Remove(Key key)
    {
      cache.RemoveKey(key);
    }

    public void Clear()
    {
      cache.Clear();
    }

    public bool Contains(Key key)
    {
      return cache.ContainsKey(key);
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

    public bool IsConsistent(Transaction current)
    {
      return transaction==current;
    }

    public void Reset(Transaction current)
    {
      Clear();
      transaction = current;
    }


    // Constructor

    public EntitySetState()
    {
      cache = new LruCache<Key, CachedKey>(CacheSize, cachedKey => cachedKey.Key);
    }
  }
}
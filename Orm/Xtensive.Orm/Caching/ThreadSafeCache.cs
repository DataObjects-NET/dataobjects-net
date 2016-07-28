// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.04.01

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Core;



namespace Xtensive.Caching
{
  /// <summary>
  /// A thread-safe wrapper for any <see cref="ICache{TKey,TItem}"/> implementation.
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  public sealed class ThreadSafeCache<TKey, TItem> : ICache<TKey, TItem>
  {
    private readonly ICache<TKey, TItem> chainedCache;
    private readonly object syncRoot;
    
    /// <summary>
    /// Gets sync root for this instance.
    /// </summary>
    public object SyncRoot { get { return syncRoot; } }

    /// <summary>
    /// Gets chained cache.
    /// </summary>
    public ICache<TKey, TItem> ChainedCache { get { return chainedCache; } }

    #region IEnumerable

    /// <inheritdoc/>
    public IEnumerator<TItem> GetEnumerator()
    {
      lock (syncRoot) {
        var enumerator = chainedCache.GetEnumerator();
        while (enumerator.MoveNext())
          yield return enumerator.Current;
      }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    #region IInvalidatable

    /// <inheritdoc/>
    public void Invalidate()
    {
      lock (syncRoot) {
        chainedCache.Invalidate();
      }
    }

    #endregion

    #region ICache<TKey, TItem>

    /// <inheritdoc/>
    public int Count { get { return chainedCache.Count; } }

    /// <inheritdoc/>
    public Converter<TItem, TKey> KeyExtractor { get { return chainedCache.KeyExtractor; } }

    /// <inheritdoc/>
    public TItem this[TKey key, bool markAsHit] {
      get {
        lock (syncRoot) {
          return chainedCache[key, markAsHit];
        }
      }
    }

    /// <inheritdoc/>
    public bool TryGetItem(TKey key, bool markAsHit, out TItem item)
    {
      lock (syncRoot) {
        return chainedCache.TryGetItem(key, markAsHit, out item);
      }
    }

    /// <inheritdoc/>
    public bool Contains(TItem item)
    {
      lock (syncRoot) {
        return chainedCache.Contains(item);
      }
    }

    /// <inheritdoc/>
    public bool ContainsKey(TKey key)
    {
      lock (syncRoot) {
        return chainedCache.ContainsKey(key);
      }
    }

    /// <inheritdoc/>
    public void Add(TItem item)
    {
      lock (syncRoot) {
        chainedCache.Add(item);
      }
    }

    /// <inheritdoc/>
    public TItem Add(TItem item, bool replaceIfExists)
    {
      lock (syncRoot) {
        return chainedCache.Add(item, replaceIfExists);
      }
    }

    /// <inheritdoc/>
    public void Remove(TItem item)
    {
      lock (syncRoot) {
        chainedCache.Remove(item);
      }
    }

    /// <inheritdoc/>
    public void RemoveKey(TKey key)
    {
      lock (syncRoot) {
        chainedCache.RemoveKey(key);
      }
    }

    public void RemoveKey(TKey key, bool removeCompletely)
    {
      RemoveKey(key);
    }

    /// <inheritdoc/>
    public void Clear()
    {
      lock (syncRoot) {
        chainedCache.Clear();
      }
    }

    #endregion


    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="chainedCache">The chained cache.</param>
    /// <param name="syncRoot">The sync root.</param>
    public ThreadSafeCache(ICache<TKey, TItem> chainedCache, object syncRoot)
    {
      ArgumentValidator.EnsureArgumentNotNull(chainedCache, "chainedCache");
      this.chainedCache = chainedCache;
      this.syncRoot = syncRoot ?? new object();
    }

    /// <summary>
    ///	Initializes new instance of this type.
    /// </summary>
    /// <param name="chainedCache">The chained cache.</param>
    public ThreadSafeCache(ICache<TKey, TItem> chainedCache)
      : this(chainedCache, null)
    {
    }
  }
}
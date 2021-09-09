// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ustinov
// Created:    2007.05.28

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Collections;
using Xtensive.Conversion;
using Xtensive.Core;


namespace Xtensive.Caching
{
  /// <summary>
  /// A set of items limited by the maximal amount of memory it can use, or by any other measure.
  /// Stores as many most frequently accessed items in memory as long as it is possible
  /// while maintaining the total size of cached items less or equal to <see cref="MaxSize"/>.
  /// </summary>
  /// <typeparam name="TKey">The key of the item.</typeparam>
  /// <typeparam name="TItem">The type of the item to cache.</typeparam>
  public class LruCache<TKey, TItem> :
    CacheBase<TKey, TItem>,
    IHasSize
  {
    private readonly long maxSize;
    private readonly TopDeque<TKey, KeyValuePair<TKey, TItem>> deque;
    private readonly Func<TItem, long> sizeExtractor;
    private readonly ICache<TKey, TItem> chainedCache;
    private long size;

    #region Properites: KeyExtractor, SizeExtractor, ChainedCache, MaxSize, Count, Size

    /// <summary>
    /// Gets the size extractor delegate.
    /// </summary>
    public Func<TItem, long> SizeExtractor {
      [DebuggerStepThrough]
      get { return sizeExtractor; }
    }


    /// <summary>
    /// Gets chained cache.
    /// </summary>
    public ICache<TKey, TItem> ChainedCache {
      [DebuggerStepThrough]
      get { return chainedCache; }
    }

    /// <inheritdoc/>
    public long MaxSize {
      [DebuggerStepThrough]
      get { return maxSize; }
    }

    /// <inheritdoc/>
    public override int Count {
      [DebuggerStepThrough]
      get { return deque.Count; }
    }

    /// <inheritdoc/>
    public long Size {
      [DebuggerStepThrough]
      get { return size; }
    }

    #endregion

    /// <inheritdoc/>
    public override bool TryGetItem(TKey key, bool markAsHit, out TItem item)
    {
      KeyValuePair<TKey, TItem> cached;
      if (deque.TryGetValue(key, markAsHit, out cached)) {
        item = cached.Value;
        return true;
      }
      if (chainedCache==null) {
        item = default(TItem);
        return false;
      }
      if (chainedCache.TryGetItem(key, false, out item)) {
        chainedCache.Remove(item);
        Add(item, true);
        return true;
      }
      return false;
    }

    /// <inheritdoc/>
    public override bool ContainsKey(TKey key)
    {
      if (deque.Contains(key))
        return true;
      if (chainedCache==null)
        return false;
      return chainedCache.ContainsKey(key);
    }

    #region Modification methods: TryAdd, Add, Remove, Clear

    /// <inheritdoc/>
    public override TItem Add(TItem item, bool replaceIfExists)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      var key = KeyExtractor(item);
      var cached = new KeyValuePair<TKey, TItem>(key, item);
      KeyValuePair<TKey, TItem> oldCached;
      if (deque.TryChangeValue(key, cached, true, replaceIfExists, out oldCached)) {
        if (!replaceIfExists)
          return oldCached.Value;
        size -= sizeExtractor(oldCached.Value);
        if (chainedCache!=null)
          chainedCache.Add(oldCached.Value, true);
        ItemRemoved(key);
      }
      size += sizeExtractor(item);
      while (size > maxSize && deque.Count > 0) {
        oldCached = deque.PopBottom();
        size -= sizeExtractor(oldCached.Value);
        if (chainedCache!=null)
          chainedCache.Add(oldCached.Value, true);
        ItemRemoved(key);
      }
      ItemAdded(key);
      return item;
    }

    /// <inheritdoc/>
    public override void RemoveKey(TKey key)
    {
      RemoveKey(key, false);
    }

    /// <inheritdoc/>
    public override void RemoveKey(TKey key, bool removeCompletely)
    {
      KeyValuePair<TKey, TItem> oldCached;
      if (deque.TryGetValue(key, out oldCached)) {
        deque.Remove(key);
        size -= sizeExtractor(oldCached.Value);
        if (chainedCache!=null) {
          if (removeCompletely)
            chainedCache.RemoveKey(key);
          else
            chainedCache.Add(oldCached.Value, true);
        }
        ItemRemoved(key);
      }
    }

    /// <inheritdoc/>
    public override void Clear()
    {
      while (deque.Count > 0) {
        var cached = deque.PopBottom();
        var key = cached.Key;
        size -= sizeExtractor(cached.Value);
        if (chainedCache!=null)
          chainedCache.Add(cached.Value, true);
        ItemRemoved(key);
      }
      size = 0;
      Cleared();
    }

    #endregion

    #region IEnumerable<...> methods

    /// <inheritdoc/>
    public override IEnumerator<TItem> GetEnumerator()
    {
      foreach (KeyValuePair<TKey, TItem> cachedItem in deque)
        yield return cachedItem.Value;
    }

    #endregion

    #region Protected events (to override)

    protected virtual void ItemAdded(TKey key) { }
    protected virtual void ItemRemoved(TKey key) { }
    protected virtual void Cleared() { }

    #endregion


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="maxSize"><see cref="MaxSize"/> property value.</param>
    /// <param name="keyExtractor"><see cref="ICache{TKey, TItem}.KeyExtractor"/> property value.</param>
    public LruCache(long maxSize, Converter<TItem, TKey> keyExtractor)
      : this(maxSize, keyExtractor, i => 1)
    {
    }

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="maxSize"><see cref="MaxSize"/> property value.</param>
    /// <param name="keyExtractor"><see cref="ICache{TKey, TItem}.KeyExtractor"/> property value.</param>
    /// <param name="sizeExtractor"><see cref="SizeExtractor"/> property value.</param>
    public LruCache(long maxSize, Converter<TItem, TKey> keyExtractor, Func<TItem, long> sizeExtractor)
      : this(maxSize, keyExtractor, sizeExtractor, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="maxSize"><see cref="MaxSize"/> property value.</param>
    /// <param name="keyExtractor"><see cref="ICache{TKey, TItem}.KeyExtractor"/> property value.</param>
    /// <param name="chainedCache"><see cref="ChainedCache"/> property value.</param>
    public LruCache(long maxSize, Converter<TItem, TKey> keyExtractor, ICache<TKey, TItem> chainedCache)
      : this(maxSize, keyExtractor, i => 1, chainedCache)
    {
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="maxSize"><see cref="MaxSize"/> property value.</param>
    /// <param name="keyExtractor"><see cref="ICache{TKey, TItem}.KeyExtractor"/> property value.</param>
    /// <param name="sizeExtractor"><see cref="SizeExtractor"/> property value.</param>
    /// <param name="chainedCache"><see cref="ChainedCache"/> property value.</param>
    public LruCache(long maxSize, Converter<TItem, TKey> keyExtractor,
      Func<TItem, long> sizeExtractor, ICache<TKey, TItem> chainedCache)
    {
      if (maxSize <= 0)
        ArgumentValidator.EnsureArgumentIsInRange(maxSize, 1, long.MaxValue, "maxSize");
      ArgumentValidator.EnsureArgumentNotNull(keyExtractor, "keyExtractor");
      ArgumentValidator.EnsureArgumentNotNull(sizeExtractor, "sizeExtractor");
      this.maxSize = maxSize;
      this.KeyExtractor = keyExtractor;
      this.sizeExtractor = sizeExtractor;
      this.chainedCache = chainedCache;
      // deque = new TopDeque<TKey, KeyValuePair<TKey, TItem>>(1 + (int) maxSize);
      deque = new TopDeque<TKey, KeyValuePair<TKey, TItem>>();
    }
  }
}

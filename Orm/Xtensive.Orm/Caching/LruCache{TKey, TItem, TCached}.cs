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
  /// <typeparam name="TCached">The type of cached representation of the item.</typeparam>
  public class LruCache<TKey, TItem, TCached> :
    ICache<TKey, TItem>,
    IHasSize
    where TCached: IIdentified<TKey>, IHasSize
  {
    private readonly long maxSize;
    private readonly TopDeque<TKey, TCached> deque;
    private readonly Converter<TItem, TKey> keyExtractor;
    private readonly Biconverter<TItem, TCached> cacheConverter;
    private readonly ICache<TKey, TItem> chainedCache;
    private long size;

    #region Properites: KeyExtractor, CacheConverter, ChainedCache, MaxSize, Count, Size

    /// <inheritdoc/>
    public Converter<TItem, TKey> KeyExtractor {
      [DebuggerStepThrough]
      get { return keyExtractor; }
    }

    /// <summary>
    /// Gets the cache converter.
    /// </summary>
    public Biconverter<TItem, TCached> CacheConverter {
      [DebuggerStepThrough]
      get { return cacheConverter; }
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
    public int Count {
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
    public TItem this[TKey key, bool markAsHit] {
      get {
        TItem item;
        if (TryGetItem(key, markAsHit, out item))
          return item;
        else
          return default(TItem);
      }
    }

    /// <inheritdoc/>
    public virtual bool TryGetItem(TKey key, bool markAsHit, out TItem item)
    {
      TCached cached;
      if (deque.TryGetValue(key, markAsHit, out cached)) {
        item = cacheConverter.ConvertBackward(cached);
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
    public bool Contains(TItem item)
    {
      return ContainsKey(KeyExtractor(item));
    }

    /// <inheritdoc/>
    public virtual bool ContainsKey(TKey key)
    {
      if (deque.Contains(key))
        return true;
      if (chainedCache==null)
        return false;
      return chainedCache.ContainsKey(key);
    }

    #region Modification methods: Add, Remove, Clear

    /// <inheritdoc/>
    public void Add(TItem item)
    {
      Add(item, true);
    }

    /// <inheritdoc/>
    public virtual TItem Add(TItem item, bool replaceIfExists)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      var key = KeyExtractor(item);
      TCached cached = cacheConverter.ConvertForward(item);
      TCached oldCached;
      if (deque.TryChangeValue(key, cached, true, replaceIfExists, out oldCached)) {
        if (!replaceIfExists)
          return cacheConverter.ConvertBackward(oldCached);
        size -= oldCached.Size;
        if (chainedCache!=null)
          chainedCache.Add(cacheConverter.ConvertBackward(oldCached), true);
        ItemRemoved(key);
      }
      size += cached.Size;
      while (size > maxSize && deque.Count > 0) {
        oldCached = deque.PopBottom();
        size -= oldCached.Size;
        if (chainedCache!=null)
          chainedCache.Add(cacheConverter.ConvertBackward(oldCached), true);
        ItemRemoved(key);
      }
      ItemAdded(key);
      return item;
    }

    /// <inheritdoc/>
    public void Remove(TItem item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      RemoveKey(KeyExtractor(item));
    }

    /// <inheritdoc/>
    public virtual void RemoveKey(TKey key)
    {
      RemoveKey(key, false);
    }

    /// <inheritdoc/>
    public void RemoveKey(TKey key, bool removeCompletely)
    {
      TCached oldCached;
      if (deque.TryGetValue(key, out oldCached)) {
        deque.Remove(key);
        size -= oldCached.Size;
        if (chainedCache!=null) {
          if (removeCompletely)
            chainedCache.RemoveKey(key);
          else
            chainedCache.Add(cacheConverter.ConvertBackward(oldCached), true);
        }
        ItemRemoved(key);
      }
    }

    /// <inheritdoc/>
    public virtual void Clear()
    {
      while (deque.Count > 0) {
        var cached = deque.PopBottom();
        var key = cached.Identifier;
        size -= cached.Size;
        if (chainedCache!=null)
          chainedCache.Add(cacheConverter.ConvertBackward(cached), true);
        ItemRemoved(key);
      }
      size = 0;
      Cleared();
    }

    /// <inheritdoc/>
    public void Invalidate()
    {
      Clear();
    }

    #endregion

    #region IEnumerable<...> methods

    /// <inheritdoc/>
    [DebuggerStepThrough]
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <inheritdoc/>
    public virtual IEnumerator<TItem> GetEnumerator()
    {
      foreach (TCached cachedItem in deque)
        yield return cacheConverter.ConvertBackward(cachedItem);
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
    /// <param name="keyExtractor"><see cref="KeyExtractor"/> property value.</param>
    public LruCache(long maxSize, Converter<TItem, TKey> keyExtractor)
      : this(maxSize, keyExtractor, Biconverter<TItem, TCached>.AsIs)
    {
    }

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="maxSize"><see cref="MaxSize"/> property value.</param>
    /// <param name="keyExtractor"><see cref="KeyExtractor"/> property value.</param>
    /// <param name="cacheConverter"><see cref="CacheConverter"/> property value.</param>
    public LruCache(long maxSize, Converter<TItem, TKey> keyExtractor, Biconverter<TItem, TCached> cacheConverter)
      : this(maxSize, keyExtractor, cacheConverter, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="maxSize"><see cref="MaxSize"/> property value.</param>
    /// <param name="keyExtractor"><see cref="KeyExtractor"/> property value.</param>
    /// <param name="chainedCache"><see cref="ChainedCache"/> property value.</param>
    public LruCache(long maxSize, Converter<TItem, TKey> keyExtractor, ICache<TKey, TItem> chainedCache)
      : this(maxSize, keyExtractor,  Biconverter<TItem, TCached>.AsIs, chainedCache)
    {
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="maxSize"><see cref="MaxSize"/> property value.</param>
    /// <param name="keyExtractor"><see cref="KeyExtractor"/> property value.</param>
    /// <param name="cacheConverter"><see cref="CacheConverter"/> property value.</param>
    /// <param name="chainedCache"><see cref="ChainedCache"/> property value.</param>
    public LruCache(long maxSize, Converter<TItem, TKey> keyExtractor, 
      Biconverter<TItem, TCached> cacheConverter, ICache<TKey, TItem> chainedCache)
    {
      if (maxSize <= 0)
        ArgumentValidator.EnsureArgumentIsInRange(maxSize, 1, long.MaxValue, "maxSize");
      ArgumentValidator.EnsureArgumentNotNull(keyExtractor, "keyExtractor");
      this.maxSize = maxSize;
      this.keyExtractor = keyExtractor;
      this.cacheConverter = cacheConverter;
      this.chainedCache = chainedCache;
      // deque = new TopDeque<TKey, TCached>(1 + (int) maxSize);
      deque = new TopDeque<TKey, TCached>();
    }
  }
}

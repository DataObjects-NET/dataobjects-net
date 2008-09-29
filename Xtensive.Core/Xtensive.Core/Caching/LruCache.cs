// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ustinov
// Created:    2007.05.28

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core.Collections;
using Xtensive.Core.Conversion;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Caching
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
    private readonly int maxSize;
    private readonly TopDeque<TKey, TCached> deque = new TopDeque<TKey, TCached>();
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

    /// <inheritdoc/>
    public ICache<TKey, TItem> ChainedCache {
      [DebuggerStepThrough]
      get { return chainedCache; }
    }

    /// <inheritdoc/>
    public int MaxSize {
      [DebuggerStepThrough]
      get { return maxSize; }
    }

    /// <inheritdoc/>
    public int Count {
      [DebuggerStepThrough]
      get { return deque.Count; }
    }

    /// <inheritdoc/>
    long ICountable.Count {
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
      if (deque.TryGetValue(key, out cached)) {
        if (markAsHit)
          deque.MoveToTop(key);
        item = cacheConverter.ConvertBackward(cached);
        return true;
      }
      if (chainedCache==null) {
        item = default(TItem);
        return false;
      }
      if (chainedCache.TryGetItem(key, false, out item)) {
        chainedCache.Remove(item);
        Add(item);
        return true;
      }
      return false;
    }

    /// <inheritdoc/>
    public virtual bool Contains(TKey key)
    {
      if (deque.Contains(key))
        return true;
      if (chainedCache==null)
        return false;
      return chainedCache.Contains(key);
    }

    #region Modification methods: Add, Remove, Clear

    /// <inheritdoc/>
    public virtual void Add(TItem item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      var key = KeyExtractor(item);
      TCached oldCached;
      if (deque.TryGetValue(key, out oldCached)) {
        deque.Remove(key);
        size -= oldCached.Size;
        if (chainedCache!=null)
          chainedCache.Add(cacheConverter.ConvertBackward(oldCached));
        ItemRemoved(key);
      }
      TCached cached = cacheConverter.ConvertForward(item);
      size += cached.Size;
      deque.AddToTop(key, cached);
      while (size > maxSize && deque.Count > 0) {
        oldCached = deque.PeekBottom();
        size -= oldCached.Size;
        if (chainedCache!=null)
          chainedCache.Add(cacheConverter.ConvertBackward(oldCached));
        ItemRemoved(key);
      }
      ItemAdded(key);
    }

    /// <inheritdoc/>
    public void Remove(TItem item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      Remove(KeyExtractor(item));
    }

    /// <inheritdoc/>
    public virtual void Remove(TKey key)
    {
      TCached oldCached;
      if (deque.TryGetValue(key, out oldCached)) {
        deque.Remove(key);
        size -= oldCached.Size;
        if (chainedCache!=null)
          chainedCache.Add(cacheConverter.ConvertBackward(oldCached));
        ItemRemoved(key);
      }
    }

    /// <inheritdoc/>
    public virtual void Clear()
    {
      while (deque.Count > 0) {
        var cached = deque.PeekBottom();
        var key = cached.Identifier;
        size -= cached.Size;
        if (chainedCache!=null)
          chainedCache.Add(cacheConverter.ConvertBackward(cached));
        ItemRemoved(key);
      }
      size = 0;
      Cleared();
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
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="maxSize"><see cref="MaxSize"/> property value.</param>
    /// <param name="keyExtractor"><see cref="KeyExtractor"/> property value.</param>
    public LruCache(int maxSize, Converter<TItem, TKey> keyExtractor)
      : this(maxSize, keyExtractor, Biconverter<TItem, TCached>.AsIs)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="maxSize"><see cref="MaxSize"/> property value.</param>
    /// <param name="keyExtractor"><see cref="KeyExtractor"/> property value.</param>
    /// <param name="cacheConverter"><see cref="CacheConverter"/> property value.</param>
    public LruCache(int maxSize, Converter<TItem, TKey> keyExtractor, Biconverter<TItem, TCached> cacheConverter)
      : this(maxSize, keyExtractor, cacheConverter, null)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="maxSize"><see cref="MaxSize"/> property value.</param>
    /// <param name="keyExtractor"><see cref="KeyExtractor"/> property value.</param>
    /// <param name="chainedCache"><see cref="ChainedCache"/> property value.</param>
    public LruCache(int maxSize, Converter<TItem, TKey> keyExtractor, ICache<TKey, TItem> chainedCache)
      : this(maxSize, keyExtractor,  Biconverter<TItem, TCached>.AsIs, chainedCache)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="maxSize"><see cref="MaxSize"/> property value.</param>
    /// <param name="keyExtractor"><see cref="KeyExtractor"/> property value.</param>
    /// <param name="cacheConverter"><see cref="CacheConverter"/> property value.</param>
    /// <param name="chainedCache"><see cref="ChainedCache"/> property value.</param>
    public LruCache(int maxSize, Converter<TItem, TKey> keyExtractor, 
      Biconverter<TItem, TCached> cacheConverter, ICache<TKey, TItem> chainedCache)
    {
      if (maxSize <= 0)
        ArgumentValidator.EnsureArgumentIsInRange(maxSize, 1, int.MaxValue, "maxSize");
      ArgumentValidator.EnsureArgumentNotNull(keyExtractor, "keyExtractor");
      this.maxSize = maxSize;
      this.keyExtractor = keyExtractor;
      this.cacheConverter = cacheConverter;
      this.chainedCache = chainedCache;
    }
  }
}

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
  public class LruCache<TKey, TItem> :
    ICache<TKey, TItem>,
    IHasSize
  {
    private readonly long maxSize;
    private readonly TopDeque<TKey, KeyValuePair<TKey, TItem>> deque = new TopDeque<TKey, KeyValuePair<TKey, TItem>>();
    private readonly Converter<TItem, TKey> keyExtractor;
    private readonly Func<TItem, long> sizeExtractor;
    private readonly ICache<TKey, TItem> chainedCache;
    private long size;

    #region Properites: KeyExtractor, SizeExtractor, ChainedCache, MaxSize, Count, Size

    /// <inheritdoc/>
    public Converter<TItem, TKey> KeyExtractor {
      [DebuggerStepThrough]
      get { return keyExtractor; }
    }

    /// <summary>
    /// Gets the size extractor delegate.
    /// </summary>
    public Func<TItem, long> SizeExtractor {
      [DebuggerStepThrough]
      get { return sizeExtractor; }
    }


    /// <inheritdoc/>
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
      KeyValuePair<TKey, TItem> cached;
      if (deque.TryGetValue(key, out cached)) {
        if (markAsHit)
          deque.MoveToTop(key);
        item = cached.Value;
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
      KeyValuePair<TKey, TItem> oldCached;
      if (deque.TryGetValue(key, out oldCached)) {
        deque.Remove(key);
        size -= sizeExtractor(oldCached.Value);
        if (chainedCache!=null)
          chainedCache.Add(oldCached.Value);
        ItemRemoved(key);
      }
      var cached = new KeyValuePair<TKey, TItem>(key, item);
      size += sizeExtractor(item);
      deque.AddToTop(key, cached);
      while (size > maxSize && deque.Count > 0) {
        oldCached = deque.PeekBottom();
        size -= sizeExtractor(oldCached.Value);
        if (chainedCache!=null)
          chainedCache.Add(oldCached.Value);
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
      KeyValuePair<TKey, TItem> oldCached;
      if (deque.TryGetValue(key, out oldCached)) {
        deque.Remove(key);
        size -= sizeExtractor(oldCached.Value);
        if (chainedCache!=null)
          chainedCache.Add(oldCached.Value);
        ItemRemoved(key);
      }
    }

    /// <inheritdoc/>
    public virtual void Clear()
    {
      while (deque.Count > 0) {
        var cached = deque.PeekBottom();
        var key = cached.Key;
        size -= sizeExtractor(cached.Value);
        if (chainedCache!=null)
          chainedCache.Add(cached.Value);
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
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="maxSize"><see cref="MaxSize"/> property value.</param>
    /// <param name="keyExtractor"><see cref="KeyExtractor"/> property value.</param>
    public LruCache(long maxSize, Converter<TItem, TKey> keyExtractor)
      : this(maxSize, keyExtractor, i => 1)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="maxSize"><see cref="MaxSize"/> property value.</param>
    /// <param name="keyExtractor"><see cref="KeyExtractor"/> property value.</param>
    /// <param name="sizeExtractor"><see cref="SizeExtractor"/> property value.</param>
    public LruCache(long maxSize, Converter<TItem, TKey> keyExtractor, Func<TItem, long> sizeExtractor)
      : this(maxSize, keyExtractor, sizeExtractor, null)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="maxSize"><see cref="MaxSize"/> property value.</param>
    /// <param name="keyExtractor"><see cref="KeyExtractor"/> property value.</param>
    /// <param name="chainedCache"><see cref="ChainedCache"/> property value.</param>
    public LruCache(long maxSize, Converter<TItem, TKey> keyExtractor, ICache<TKey, TItem> chainedCache)
      : this(maxSize, keyExtractor, i => 1, chainedCache)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="maxSize"><see cref="MaxSize"/> property value.</param>
    /// <param name="keyExtractor"><see cref="KeyExtractor"/> property value.</param>
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
      this.keyExtractor = keyExtractor;
      this.sizeExtractor = sizeExtractor;
      this.chainedCache = chainedCache;
    }
  }
}

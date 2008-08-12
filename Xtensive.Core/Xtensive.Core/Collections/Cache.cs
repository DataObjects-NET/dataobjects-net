// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ustinov
// Created:    2007.05.28

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core.Conversion;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Resources;

namespace Xtensive.Core.Collections
{
  /// <summary>
  /// A set of items limited by the maximal amount of memory it can use, or by any other measure.
  /// Stores as many most frequently accessed items in memory as it is possible
  /// while maintaining the total size of cached items less or equal to <see cref="MaxSize"/>.
  /// </summary>
  /// <typeparam name="TKey">The key of the item.</typeparam>
  /// <typeparam name="TItem">The type of the item to cache.</typeparam>
  /// <typeparam name="TCached">The type of cached representation of the item.</typeparam>
  public class Cache<TKey, TItem, TCached> :
    ICache<TKey, TItem>
    where TCached: IIdentified<TKey>, IHasSize
  {
    private readonly int maxSize;
    private readonly TopDeque<TKey, TCached> deque = new TopDeque<TKey, TCached>();
    private readonly Converter<TItem, TKey> keyExtractor;
    private readonly Biconverter<TItem, TCached> cacheConverter;
    private long size;

    #region Properites: KeyExtractor, CacheConverter, MaxSize, Count, Size

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public Converter<TItem, TKey> KeyExtractor
    {
      get { return keyExtractor; }
    }

    /// <summary>
    /// Gets the cache converter.
    /// </summary>
    [DebuggerStepThrough]
    public Biconverter<TItem, TCached> CacheConverter
    {
      get { return cacheConverter; }
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public int MaxSize
    {
      get { return maxSize; }
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public long Count
    {
      get { return deque.Count; }
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public long Size
    {
      get { return size; }
    }

    #endregion

    /// <inheritdoc/>
    public TItem this[TKey key, bool markAsNewest] {
      get {
        if (!deque.Contains(key))
          return default(TItem);
        var item = deque[key];
        if (markAsNewest)
          deque.MoveToTop(key);
        return cacheConverter.ConvertBackward(item);
      }
    }

    /// <inheritdoc/>
    public bool Contains(TKey key)
    {
      return deque.Contains(key);
    }

    #region Modification methods: Add, Remvoe, Clear

    /// <inheritdoc/>
    public void Add(TItem item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      var key = KeyExtractor(item);
      var oldCachedExists = deque.Contains(key);
      var oldCached = oldCachedExists ? deque[key] : default(TCached);
      if (oldCachedExists) {
        size -= oldCached.Size;
        deque.Remove(key);
        ItemRemoved(key);
      }
      TCached cached = cacheConverter.ConvertForward(item);
      size += cached.Size;
      deque.AddToTop(key, cached);
      while (size > maxSize && deque.Count > 0) {
        oldCached = deque.PeekBottom();
        size -= oldCached.Size;
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
    public void Remove(TKey key)
    {
      var oldCachedExists = deque.Contains(key);
      var oldCached = oldCachedExists ? deque[key] : default(TCached);
      if (oldCachedExists) {
        size -= oldCached.Size;
        deque.Remove(key);
        ItemRemoved(key);
      }
    }

    /// <inheritdoc/>
    public void Clear()
    {
      deque.Clear();
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
    public IEnumerator<TItem> GetEnumerator()
    {
      foreach (TCached itemClone in deque)
        yield return cacheConverter.ConvertBackward(itemClone);
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
    public Cache(int maxSize, Converter<TItem, TKey> keyExtractor)
    {
      if (maxSize <= 0)
        ArgumentValidator.EnsureArgumentIsInRange(maxSize, 0, int.MaxValue, "maxSize");
      ArgumentValidator.EnsureArgumentNotNull(keyExtractor, "keyExtractor");
      this.maxSize = maxSize;
      this.keyExtractor = keyExtractor;
      cacheConverter = new Biconverter<TItem, TCached>(
        value => (TCached)(object)value,
        value => (TItem)(object)value);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="maxSize"><see cref="MaxSize"/> property value.</param>
    /// <param name="cacheConverter"><see cref="CacheConverter"/> property value.</param>
    /// <param name="keyExtractor"><see cref="KeyExtractor"/> property value.</param>
    public Cache(int maxSize, Converter<TItem, TKey> keyExtractor, Biconverter<TItem, TCached> cacheConverter)
      : this (maxSize, keyExtractor)
    {
      this.cacheConverter = cacheConverter;
    }
  }
}

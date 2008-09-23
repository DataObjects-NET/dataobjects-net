// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.05.30

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Xtensive.Core.Conversion;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Threading;

namespace Xtensive.Core.Collections
{
  /// <summary>
  /// A set of items limited by the maximal amount of memory it can use, or by any other measure.
  /// Keeps strong references to as many most frequently accessed items as it is possible
  /// while maintaining the total size of strongly referenced items less or equal to <see cref="MaxSize"/>.
  /// All the items that do not fit into the strongly referenced "part" of this cache are weekly referenced, 
  /// so many of them can be available in it as well.
  /// </summary>
  /// <typeparam name="TKey">The key of the item.</typeparam>
  /// <typeparam name="TItem">The type of the item to cache.</typeparam>
  public class WeakCache<TKey, TItem> : 
    ICache<TKey, TItem>
    where TItem : class
  {
    #region Nested type: WeakCacheItem

    private class WeakCacheItem :
      IIdentified<TKey>,
      IHasSize
    {
      public WeakCache<TKey, TItem> Cache { get; private set; }
      public TItem Item { get; private set; }
      public long Size { get; private set; }

      object IIdentified.Identifier
      {
        [DebuggerStepThrough]
        get { return Identifier; }
      }

      public TKey Identifier
      {
        [DebuggerStepThrough]
        get { return Cache.KeyExtractor(Item); }
      }


      // Constructors

      public WeakCacheItem(WeakCache<TKey, TItem> cache, TItem item, long size)
      {
        Cache = cache;
        Item = item;
        Size = size;
      }
    }

    #endregion

    private readonly Func<TItem, long> sizeExtractor;
    private readonly Cache<TKey, TItem, WeakCacheItem> cache;
    private readonly WeakDictionary<TKey, TItem> weakMap = new WeakDictionary<TKey, TItem>();

    #region Properties: MaxSize, Count, Size, KeyExtractor, SizeExtractor

    /// <summary>
    /// Gets the maximal size of strongly referenced cached items.
    /// </summary>
    public int MaxSize
    {
      [DebuggerStepThrough]
      get { return cache.MaxSize; }
    }

    /// <summary>
    /// <inheritdoc/>
    /// This property returns the count of strongly referenced cached items.
    /// </summary>
    public long Count
    {
      [DebuggerStepThrough]
      get { return cache.Count; }
    }

    /// <summary>
    /// <inheritdoc/>
    /// This property returns the size of strongly referenced cached items.
    /// </summary>
    public long Size
    {
      [DebuggerStepThrough]
      get { return cache.Size; }
    }

    /// <inheritdoc/>
    public Converter<TItem, TKey> KeyExtractor
    {
      [DebuggerStepThrough]
      get { return cache.KeyExtractor; }
    }

    /// <summary>
    /// Gets the item size extractor.
    /// </summary>
    public Func<TItem, long> SizeExtractor
    {
      [DebuggerStepThrough]
      get { return sizeExtractor; }
    }

    #endregion

    /// <inheritdoc/>
    public TItem this[TKey key, bool markAsNewest] {
      get {
        TItem result;
        // Let's check the cache first
        if (cache.Contains(key))
          return cache[key, markAsNewest];
        // Nothing is found - let's try weakMap
        if (weakMap.TryGetValue(key, out result)) {
          if (markAsNewest)
            cache.Add(result);
          return result;
        }
        // Nothing at all
        return default(TItem);
      }
    }

    /// <inheritdoc/>
    public bool Contains(TKey key)
    {
      return cache.Contains(key) || weakMap.ContainsKey(key);
    }

    #region Modification methods: Add, Remove, Clear

    /// <inheritdoc/>
    public void Add(TItem item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      TKey key = KeyExtractor(item);
      cache.Add(item);
      weakMap.Add(key, item);
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
      cache.Remove(key);
      weakMap.Remove(key);
    }

    /// <inheritdoc/>
    public void Clear()
    {
      cache.Clear();
      weakMap.Clear();
    }

    #endregion

    #region GetEnumerator<...> methods

    /// <summary>
    /// <inheritdoc/>
    /// This method returns the sequence of just strongly referenced cached items.
    /// </summary>
    [DebuggerStepThrough]
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <summary>
    /// <inheritdoc/>
    /// This method returns the sequence of just strongly referenced cached items.
    /// </summary>
    public IEnumerator<TItem> GetEnumerator()
    {
      return cache.GetEnumerator();
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="maxSize"><see cref="MaxSize"/> property value.</param>
    /// <param name="keyExtractor"><see cref="KeyExtractor"/> property value.</param>
    public WeakCache(int maxSize, Converter<TItem, TKey> keyExtractor)
      : this(maxSize, keyExtractor, item => {
        var hasSize = item as IHasSize;
        if (hasSize!=null)
          return hasSize.Size;
        return 1;
      })
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="maxSize"><see cref="MaxSize"/> property value.</param>
    /// <param name="keyExtractor"><see cref="KeyExtractor"/> property value.</param>
    /// <param name="sizeExtractor"><see cref="SizeExtractor"/> property value.</param>
    public WeakCache(int maxSize, Converter<TItem, TKey> keyExtractor, Func<TItem, long> sizeExtractor)
    {
      if (maxSize <= 0)
        ArgumentValidator.EnsureArgumentIsInRange(maxSize, 0, int.MaxValue, "maxSize");
      ArgumentValidator.EnsureArgumentNotNull(keyExtractor,  "keyExtractor");
      ArgumentValidator.EnsureArgumentNotNull(sizeExtractor, "sizeExtractor");
      this.sizeExtractor = sizeExtractor;
      cache = new Cache<TKey, TItem, WeakCacheItem>(maxSize, keyExtractor, 
        new Biconverter<TItem, WeakCacheItem>(
          item => new WeakCacheItem(this, item, sizeExtractor(item)), 
          packedItem => packedItem.Item
        ));
    }
  }
}
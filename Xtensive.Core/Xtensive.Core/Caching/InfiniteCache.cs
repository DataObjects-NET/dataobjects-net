// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.11

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Resources;

namespace Xtensive.Core.Caching
{

  /// <summary>
  /// An unlimited set of items.
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  public class InfiniteCache<TKey, TItem>:
    ICache<TKey, TItem>
    where TItem : class 
  {
    private readonly Dictionary<TKey, TItem> items;
    private readonly Converter<TItem, TKey> keyExtractor;

    #region Implementation of IEnumerable

    /// <inheritdoc/>
    public IEnumerator<TItem> GetEnumerator()
    {
      foreach (var pair in items) {
        yield return pair.Value;
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    #region Implementation of ICountable

    /// <inheritdoc/>
    long ICountable.Count
    {
      get { return Count; }
    }
    
    #endregion
    
    #region Implementation of ICache<TKey,TItem>

    /// <inheritdoc/>
    public Converter<TItem, TKey> KeyExtractor
    {
      get { return keyExtractor; }
    }

    /// <inheritdoc/>
    public ICache<TKey, TItem> ChainedCache
    {
      get { return null; }
    }

    /// <inheritdoc/>
    public TItem this[TKey key, bool markAsHit]
    {
      get
      {
        TItem item;
        if (items.TryGetValue(key, out item)) {
          return item;
        }

        return null;
      }
    }

    /// <inheritdoc/>
    public bool TryGetItem(TKey key, bool markAsHit, out TItem item)
    {
      return items.TryGetValue(key, out item);
    }

    /// <inheritdoc/>
    public bool Contains(TItem item)
    {
      return items.ContainsValue(item);
    }

    /// <inheritdoc/>
    public bool ContainsKey(TKey key)
    {
      return items.ContainsKey(key);
    }

    /// <inheritdoc/>
    public void Add(TItem item)
    {
      Add(item, true);
    }

    /// <inheritdoc/>
    public TItem Add(TItem item, bool replaceIfExists)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      var key = KeyExtractor(item);
      
      TItem cachedItem;
      if (!replaceIfExists && items.TryGetValue(key, out cachedItem)) {
          return cachedItem;
      }

      items[key] = item;
      return item;
    }

    /// <inheritdoc/>
    public void Remove(TItem item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      RemoveKey(KeyExtractor(item));
    }

    /// <inheritdoc/>
    public void RemoveKey(TKey key)
    {
      if (items.ContainsKey(key))
        items.Remove(key);
    }

    /// <inheritdoc/>
    public void Clear()
    {
      items.Clear();
    }

    /// <inheritdoc/>
    public int Count
    {
      get { return items.Count; }
    }

    #endregion

    // Constructors

    /// <summary>
    /// 	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="keyExtractor"><see cref="KeyExtractor"/> property value.</param>
    public InfiniteCache(Converter<TItem, TKey> keyExtractor)
      : this(0, keyExtractor)
    {
    }

    /// <summary>
    /// 	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="capacity">The capacity of cache.</param>
    /// <param name="keyExtractor"><see cref="KeyExtractor"/> property value.</param>
    public InfiniteCache(int capacity, Converter<TItem, TKey> keyExtractor)
    {
      ArgumentValidator.EnsureArgumentNotNull(keyExtractor, "keyExtractor");
      if (capacity < 0)
        throw new ArgumentOutOfRangeException("capacity", capacity, Strings.ExArgumentValueMustBeGreaterThanOrEqualToZero);

      this.keyExtractor = keyExtractor;
      items = new Dictionary<TKey, TItem>(capacity);
    }

  }
}
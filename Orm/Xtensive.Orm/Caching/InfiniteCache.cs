// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.11

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Core;



namespace Xtensive.Caching
{

  /// <summary>
  /// An unlimited set of items.
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  public sealed class InfiniteCache<TKey, TItem>:
    ICache<TKey, TItem>
    where TItem : class 
  {
    private readonly Dictionary<TKey, TItem> items;
    private readonly Converter<TItem, TKey> keyExtractor;

    /// <inheritdoc/>
    public Converter<TItem, TKey> KeyExtractor
    {
      get { return keyExtractor; }
    }

    /// <inheritdoc/>
    public int Count
    {
      get { return items.Count; }
    }

    /// <inheritdoc/>
    public TItem this[TKey key, bool markAsHit] {
      get {
        TItem item;
        if (items.TryGetValue(key, out item))
          return item;
        else
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
      if (!replaceIfExists && items.TryGetValue(key, out cachedItem))
        return cachedItem;

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
    public void RemoveKey(TKey key, bool removeCompletely)
    {
      RemoveKey(key);
    }

    /// <inheritdoc/>
    public void Clear()
    {
      items.Clear();
    }

    /// <inheritdoc/>
    public void Invalidate()
    {
      Clear();
    }

    #region IEnumerable methods

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


    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="keyExtractor"><see cref="KeyExtractor"/> property value.</param>
    public InfiniteCache(Converter<TItem, TKey> keyExtractor)
      : this(0, keyExtractor)
    {
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="capacity">The capacity of cache.</param>
    /// <param name="keyExtractor"><see cref="KeyExtractor"/> property value.</param>
    /// <exception cref="ArgumentOutOfRangeException"><c>capacity</c> is out of range.</exception>
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
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
    CacheBase<TKey, TItem>
    where TItem : class
  {
    private readonly Dictionary<TKey, TItem> items;

    /// <inheritdoc/>
    public override int Count
    {
      get { return items.Count; }
    }

    /// <inheritdoc/>
    public override bool TryGetItem(TKey key, bool markAsHit, out TItem item)
    {
      return items.TryGetValue(key, out item);
    }

    /// <inheritdoc/>
    public bool Contains(TItem item)
    {
      return items.ContainsValue(item);
    }

    /// <inheritdoc/>
    public override bool ContainsKey(TKey key)
    {
      return items.ContainsKey(key);
    }

    /// <inheritdoc/>
    public override TItem Add(TItem item, bool replaceIfExists)
    {
      ArgumentNullException.ThrowIfNull(item, "item");
      var key = KeyExtractor(item);

      TItem cachedItem;
      if (!replaceIfExists && items.TryGetValue(key, out cachedItem))
        return cachedItem;

      items[key] = item;
      return item;
    }

    /// <inheritdoc/>
    public override void RemoveKey(TKey key)
    {
      _ = items.Remove(key);
    }

    /// <inheritdoc/>
    public override void RemoveKey(TKey key, bool removeCompletely)
    {
      RemoveKey(key);
    }

    /// <inheritdoc/>
    public override void Clear()
    {
      items.Clear();
    }

    #region IEnumerable methods

    /// <inheritdoc/>
    public override IEnumerator<TItem> GetEnumerator()
    {
      foreach (var pair in items) {
        yield return pair.Value;
      }
    }

    #endregion


    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="keyExtractor"><see cref="ICache{TKey, TItem}.KeyExtractor"/> property value.</param>
    public InfiniteCache(Converter<TItem, TKey> keyExtractor)
      : this(0, keyExtractor)
    {
    }

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="capacity">The capacity of cache.</param>
    /// <param name="keyExtractor"><see cref="ICache{TKey, TItem}.KeyExtractor"/> property value.</param>
    /// <exception cref="ArgumentOutOfRangeException"><c>capacity</c> is out of range.</exception>
    public InfiniteCache(int capacity, Converter<TItem, TKey> keyExtractor)
    {
      ArgumentNullException.ThrowIfNull(keyExtractor, "keyExtractor");
      if (capacity < 0)
        throw new ArgumentOutOfRangeException("capacity", capacity, Strings.ExArgumentValueMustBeGreaterThanOrEqualToZero);
      this.KeyExtractor = keyExtractor;
      items = new Dictionary<TKey, TItem>(capacity);
    }
  }
}
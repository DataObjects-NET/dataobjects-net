// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.05.25

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core.Conversion;
using Xtensive.Core.SizeCalculators;
using Xtensive.Core.Threading;

namespace Xtensive.Core.Collections
{
  public interface ICache<TKey, TItem> : 
    ICountable<TItem>,
    IHasSize
  {
    /// <summary>
    /// Gets the item key extractor.
    /// </summary>
    Converter<TItem, TKey> KeyExtractor { get; }

    /// <summary>
    /// Gets cached item by its <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key of the item to get.</param>
    /// <param name="markAsNewest">Indicates whether the item with specified key 
    /// should be marked as the newest one.</param>
    /// <returns>Item, if found; 
    /// otherwise, <see langword="default(TItem)"/>.</returns>
    TItem this[TKey key, bool markAsNewest] { get; }

    /// <summary>
    /// Determines whether cache contains the item with specified key.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>
    /// <see langword="True"/> if cache the item with specified key; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool Contains(TKey key);

    /// <summary>
    /// Adds a new item to the cache. If item with this key is already in cache - replaces is with new item.
    /// </summary>
    /// <param name="item">The item to add.</param>
    void Add(TItem item);

    /// <summary>
    /// Removes the specified <paramref name="item"/> from the cache.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    void Remove(TItem item);
  
    /// <summary>
    /// Removes the item with specified <paramref name="key"/> from the cache.
    /// </summary>
    /// <param name="key">The key of the item to remove.</param>
    void Remove(TKey key);
  
    /// <summary>
    ///  Clears the cache.
    /// </summary>
    void Clear();
  }
}
// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.05.25

using System;
using System.Collections.Generic;

namespace Xtensive.Caching
{
  /// <summary>
  /// Cache contract.
  /// </summary>
  /// <typeparam name="TKey">The type of the cache key.</typeparam>
  /// <typeparam name="TItem">The type of the item to cached.</typeparam>
  public interface ICache<TKey, TItem> : IEnumerable<TItem>, IInvalidatable
  {
    /// <summary>
    /// Gets the count of cached items.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Gets the item key extractor.
    /// </summary>
    Converter<TItem, TKey> KeyExtractor { get; }

    /// <summary>
    /// Gets cached item by its <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key of the item to get.</param>
    /// <param name="markAsHit">Indicates whether the item with specified key 
    /// should be marked as hit.</param>
    /// <returns>Item, if found; 
    /// otherwise, <see langword="default(TItem)"/>.</returns>
    TItem this[TKey key, bool markAsHit] { get; }

    /// <summary>
    /// Tries to get cached item by its <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key of the item to get.</param>
    /// <param name="markAsHit">Indicates whether the item with specified key
    /// should be marked as hit.</param>
    /// <param name="item">The item, if found.</param>
    /// <returns>
    /// <see langword="true" />, if the item is found;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool TryGetItem(TKey key, bool markAsHit, out TItem item);

    /// <summary>
    /// Determines whether cache contains the specified item.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <returns>
    /// <see langword="True"/> if cache contains the specified item; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool Contains(TItem item);

    /// <summary>
    /// Determines whether cache contains the item with specified key.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>
    /// <see langword="True"/> if cache contains the item with specified key; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool ContainsKey(TKey key);

    /// <summary>
    /// Adds a new item to the cache. If item with this key is already in cache - replaces is with new item.
    /// </summary>
    /// <param name="item">The item to add.</param>
    void Add(TItem item);

    /// <summary>
    /// Adds a new item to the cache.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <param name="replaceIfExists">Indicates whether existing item must be replaced or not.</param>
    /// <returns>An existing, or a newly added item.</returns>
    TItem Add(TItem item, bool replaceIfExists);

    /// <summary>
    /// Removes the specified <paramref name="item"/> from the cache.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    void Remove(TItem item);
  
    /// <summary>
    /// Removes the item with specified <paramref name="key"/> from the cache.
    /// </summary>
    /// <param name="key">The key of the item to remove.</param>
    void RemoveKey(TKey key);

    /// <summary>
    /// Removes the item with specified <paramref name="key"/> from the cache.
    /// </summary>
    /// <param name="key">The key of the item to remove</param>
    /// <param name="removeCompletely">Indicates whether the item with specified key should be removed from inner caches</param>
    void RemoveKey(TKey key, bool removeCompletely);
  
    /// <summary>
    ///  Clears the cache.
    /// </summary>
    void Clear();
  }
}
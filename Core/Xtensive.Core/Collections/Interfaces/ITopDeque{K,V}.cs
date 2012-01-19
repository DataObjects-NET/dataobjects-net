// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.26

namespace Xtensive.Collections
{
  /// <summary>
  /// "Top deque" contract - a combination of double-ended queue and dictionary.
  /// This class is normally used to build LRU caches.
  /// </summary>
  /// <typeparam name="K">The type of the key.</typeparam>
  /// <typeparam name="V">The type of the value.</typeparam>
  public interface ITopDeque<K,V> : ICountable<V>
  {
    /// <summary>
    /// Gets the count of items.
    /// </summary>
    new int Count { get; }

    /// <summary>
    /// Gets or sets the item with the specified key.
    /// </summary>
    /// <value>The item with the specified key.</value>
    V this[K key] { get; set; }

    /// <summary>
    /// Tries the to get the value by its key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns>
    /// <see langword="true"/>, if operation was successful;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool TryGetValue(K key, out V value);

    /// <summary>
    /// Tries the to get the value by its key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="moveToTop">Indicates whether item must be moved to the top, if found.</param>
    /// <param name="value">The value.</param>
    /// <returns>
    /// <see langword="true"/>, if operation was successful;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool TryGetValue(K key, bool moveToTop, out V value);

    /// <summary>
    /// Tries to the change value.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <param name="moveToTop">Indicates whether item must be moved to the top, if found.</param>
    /// <param name="replaceIfExists">Indicates whether value must be replaced, if specified key is found.</param>
    /// <param name="oldValue">The old value.</param>
    /// <returns>
    /// <see langword="true"/>, if change occurred;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool TryChangeValue(K key, V value, bool moveToTop, bool replaceIfExists, out V oldValue);

    /// <summary>
    /// Determines whether collection contains the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>
    /// <see langword="true"/>, if specified key is found;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool Contains(K key);

    /// <summary>
    /// Gets the top item without removing it from the collection.
    /// </summary>
    V Top { get; }

    /// <summary>
    /// Gets the bottom item without removing it from the collection.
    /// </summary>
    V Bottom { get; }

    /// <summary>
    /// Gets the top key without removing it from the collection.
    /// </summary>
    K TopKey { get; }

    /// <summary>
    /// Gets the bottom key without removing it from the collection.
    /// </summary>
    K BottomKey { get; }

    /// <summary>
    /// Gets the top item and removes it from the collection.
    /// </summary>
    V PopTop();

    /// <summary>
    /// Gets the top item and removes it from the collection.
    /// </summary>
    V PopBottom();

    /// <summary>
    /// Moves the item with the specified key to top.
    /// </summary>
    /// <param name="key">The key.</param>
    void MoveToTop(K key);

    /// <summary>
    /// Moves the item with the specified key to bottom.
    /// </summary>
    /// <param name="key">The key.</param>
    void MoveToBottom(K key);

    /// <summary>
    /// Adds the new item to top.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    void AddToTop(K key, V value);

    /// <summary>
    /// Adds the new item to bottom.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    void AddToBottom(K key, V value);

    /// <summary>
    /// Removes the item with the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    void Remove(K key);

    /// <summary>
    /// Clears this collection.
    /// </summary>
    void Clear();
  }
}
// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.05.24

using System;
using System.Collections.Generic;

namespace Xtensive.Collections
{
  /// <summary>
  /// Keyed pool contract.
  /// </summary>
  /// <typeparam name="K">The type of key to retrieve the items by.</typeparam>
  /// <typeparam name="T">The type of pooled item.</typeparam>
  public interface IPool<K,T>: IPoolBase<T>, IEnumerable<T>
  {
    /// <summary>
    /// Adds the item into the pool.
    /// </summary>
    /// <param name="key">Key to associate the item with.</param>
    /// <param name="item">Item to add.</param>
    /// <returns><see langword="True"/>, if the item was successfully added;
    /// otherwise (e.g. if the pool is full), <see langword="false"/>.</returns>
    bool Add(K key, T item);

    /// <summary>
    /// Consumes the item with specified key from the pool.
    /// </summary>
    /// <param name="key">Key to get the pooled item for.</param>
    /// <returns>Pooled item.</returns>
    T    Consume(K key);

    /// <summary>
    /// Consumes the item with specified key from the pool;
    /// creates a new one (using <paramref name="itemGenerator"/>),
    /// if no available item is found in pool.
    /// </summary>
    /// <param name="key">Key to get the pooled item for.</param>
    /// <param name="itemGenerator">Item generator to use, if no pooled item is found.</param>
    /// <returns>Pooled or newly created item.</returns>
    T    Consume(K key, Func<T> itemGenerator);

    /// <summary>
    /// Adds the specified item into the pool and immediately consumes it.
    /// </summary>
    /// <param name="key">Key to associate the item with.</param>
    /// <param name="item">Item to add and consume.</param>
    void Consume(K key, T item);

    /// <summary>
    /// Executes <paramref name="consumer"/> action for either
    /// pooled or newly created item. 
    /// <see cref="IPoolBase{T}.Release"/>s the item on completion of
    /// execution.
    /// </summary>
    /// <param name="key">Key to get the pooled item for.</param>
    /// <param name="itemGenerator">Item generator to use, if no pooled item is found.</param>
    /// <param name="consumer">The action to execute.</param>
    void ExecuteConsumer(K key, Func<T> itemGenerator, Action<K,T> consumer);

    /// <summary>
    /// Removes all the items associated with the specified
    /// <paramref name="key"/> from the pool.
    /// </summary>
    /// <param name="key">Key to remove the pooled items for.</param>
    /// <returns>The array of items that were removed from the pool.</returns>
    T[] RemoveKey(K key);
  }
}
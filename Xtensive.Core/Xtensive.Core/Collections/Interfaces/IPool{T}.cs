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
  /// Pool contract.
  /// </summary>
  /// <typeparam name="T">The type of pooled item.</typeparam>
  public interface IPool<T>: IPoolBase<T>, IEnumerable<T>
  {
    /// <summary>
    /// Adds the item into the pool.
    /// </summary>
    /// <param name="item">Item to add.</param>
    /// <returns><see langword="True"/>, if the item was successfully added;
    /// otherwise (e.g. if the pool is full), <see langword="false"/>.</returns>
    bool Add(T item);

    /// <summary>
    /// Consumes the item from the pool.
    /// </summary>
    /// <returns>Pooled item.</returns>
    T    Consume();

    /// <summary>
    /// Adds the specified item into the pool and immediately consumes it.
    /// </summary>
    /// <param name="item">Item to add and consume.</param>
    void Consume(T item);

    /// <summary>
    /// Consumes the item from the pool;
    /// creates a new one (using <paramref name="itemGenerator"/>),
    /// if no available item is found in pool.
    /// </summary>
    /// <param name="itemGenerator">Item generator to use, if no pooled item is found.</param>
    /// <returns>Pooled or newly created item.</returns>
    T    Consume(Func<T> itemGenerator);

    /// <summary>
    /// Executes <paramref name="consumer"/> action for either
    /// pooled or newly created item.
    /// <see cref="IPoolBase{T}.Release"/>s the item on completion of
    /// execution.
    /// </summary>
    /// <param name="itemGenerator">Item generator to use, if no pooled item is found.</param>
    /// <param name="consumer">The action to execute.</param>
    void ExecuteConsumer(Func<T> itemGenerator, Action<T> consumer);
  }
}
// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.06.13

using System;
using Xtensive.Threading;

namespace Xtensive.Collections
{
  /// <summary>
  /// Base interface for pools.
  /// </summary>
  /// <typeparam name="T">The type of pooled item.</typeparam>
  public interface IPoolBase<T>
  {
    /// <summary>
    /// Gets the capacity of the pool.
    /// </summary>
    int Capacity { get; }

    /// <summary>
    /// Gets the count of available items in the pool.
    /// </summary>
    int AvailableCount { get; }

    /// <summary>
    /// Gets the total count of items in the pool.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Removes the specified <paramref name="item"/> from the pool.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <returns><see langword="True"/>, if the item was successfully removed;
    /// otherwise (e.g. if there were no such item), <see langword="false"/>.</returns>
    bool Remove(T item);

    /// <summary>
    /// Returns the pooled item back to the pool.
    /// </summary>
    /// <param name="item">The item to release.</param>
    void Release(T item);

    /// <summary>
    /// Gets a value indicating whether specified <paramref name="item"/> is pooled.
    /// </summary>
    /// <param name="item">Item to check the status for.</param>
    /// <returns><see langword="True"/>, if the specified item is pooled;
    /// otherwise (e.g. if there is no such item), <see langword="false"/>.</returns>
    bool IsPooled(T item);

    /// <summary>
    /// Gets a value indicating whether specified <paramref name="item"/> is pooled and available now.
    /// </summary>
    /// <param name="item">Item to check the status for.</param>
    /// <returns><see langword="True"/>, if the specified item is pooled and available;
    /// otherwise (e.g. if there is no such item, or it is consumed now), <see langword="false"/>.</returns>
    bool IsAvailable(T item);

    /// <summary>
    /// Gets a value indicating whether specified <paramref name="item"/> is pooled and consumed now.
    /// </summary>
    /// <param name="item">Item to check the status for.</param>
    /// <returns><see langword="True"/>, if the specified item is pooled and consumed now;
    /// otherwise (e.g. if there is no such item, or it is available now), <see langword="false"/>.</returns>
    bool IsConsumed(T item);

    /// <summary>
    /// Occurs after item is removed from the pool.
    /// </summary>
    event EventHandler<ItemRemovedEventArgs<T>> ItemRemoved;
  }
}
// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.06.13

using System;
using System.Collections.Generic;
using Xtensive.Core;


namespace Xtensive.Collections
{
  /// <summary>
  /// Priority queue interface.
  /// </summary>
  /// <typeparam name="T"><see cref="Type"/> of items to be stored in queue.</typeparam>
  /// <typeparam name="TPriority"><see cref="Type"/> of priority value.</typeparam>
  public interface IPriorityQueue<T, TPriority> : 
    ICountable<T>, ICloneable
    where TPriority : IComparable<TPriority>
  {
    /// <summary>
    /// Gets direction of items stored in the queue
    /// </summary>
    Direction Direction { get; }

    /// <summary>
    /// Adds an <paramref name="item"/> to the priority queue.
    /// </summary>
    /// <param name="item">Item to be added to the queue.</param>
    /// <param name="priority">Priority value.</param>
    void Enqueue(T item, TPriority priority);

    /// <summary>
    /// Removes top item from the queue.
    /// </summary>
    /// <returns>Top item from priority queue if applicable.</returns>
    T Dequeue();

    /// <summary>
    /// Removes range of items from queue. Item's priority must be greater than <paramref name="priority"/>.
    /// </summary>
    /// <param name="priority">Threshold value for items to remove from queue </param>
    /// <returns>An <see langword="array"/> of items. Empty <see langword="array"/> if no items found under <paramref name="priority"/> condition.</returns>
    T[] DequeueRange(TPriority priority);

    /// <summary>
    /// Returns top item from the priority queue but not removes it from the queue.
    /// </summary>
    /// <returns>Top item from queue if applicable.</returns>
    T Peek();

    /// <summary>
    /// Gets or sets the size of an internal array.
    /// </summary>
    int Capacity { get; set; }

    /// <summary>
    /// Sets the capacity to the actual number of elements in the queue,
    /// </summary>
    void TrimExcess();

    /// <summary>
    /// Removes all objects from the queue.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="ICountable.Count"/> is set to zero, and references to other objects 
    /// from elements of the queue are also released.
    /// </para>
    /// <para>
    /// The capacity remains unchanged. To reset the capacity of the queue,
    /// call <see cref="TrimExcess()"/>. Trimming an empty queue sets the capacity 
    /// of the queue to the default capacity.
    /// </para>
    /// <para>
    /// This method is an O(n) operation, where n is <see cref="ICountable.Count"/>.
    /// </para>
    /// </remarks>
    void Clear();

    /// <summary>
    /// Gets the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get.</param>
    /// <exception cref="ArgumentOutOfRangeException">The specified <paramref name="index"/> 
    /// is less than zero or is equal to \ greater than <see langword="Count"/>.</exception>
    T this[int index] { get; }

    /// <summary>
    /// Determines whether an element is in the queue. 
    /// </summary>
    /// <param name="item">
    /// The element to locate in the queue. The value can be a 
    /// <see langword="null"/> for reference types.
    /// </param>
    /// <returns><see langword="true"/> if <paramref name="item"/> is found in the 
    /// queue; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// <para>
    /// This method determines equality using the default equality comparer 
    /// <see cref="EqualityComparer{T}.Default"/> for <typeparamref name="T"/>,
    /// the type of values in the queue.
    /// </para>
    /// <para>
    /// This method performs a linear search; therefore, this method is an O(n) operation,
    /// where n is <see cref="ICountable.Count"/>.
    /// </para>
    /// </remarks>
    bool Contains(T item);


    /// <summary>
    /// Removes an element from the queue
    /// </summary>
    /// <param name="item">The element to remove from the queue</param>
    void Remove(T item);
  }
}
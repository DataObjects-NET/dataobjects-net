// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.01.07

using System;
using System.Collections;
using System.Collections.Generic;

namespace Xtensive.Collections
{
  /// <summary>
  /// Double-ended queue contract.
  /// </summary>
  /// <typeparam name="T">The type of queued elements.</typeparam>
  public interface IDeque<T> : IList<T>, IList, ICountable<T>
  {
    /// <summary>
    /// Gets or sets the size of an internal array.
    /// </summary>
    /// <value>
    /// Indicates the size of an internal array.
    /// The minimally allowed value which is also the default one is 16.
    /// </value>
    /// <remarks>
    /// As elements are added to a <see cref="IDeque{T}"/>, the capacity is automatically increased as required
    /// by reallocating the internal array. The capacity can be decreased by calling <see cref="IDeque{T}.TrimExcess"/>.
    /// </remarks>
    int Capacity { get; set; }

    /// <summary>
    /// Sets the capacity to the actual number of elements in the <see cref="IDeque{T}"/>,
    /// if that number is less than 90 percent of current capacity.
    /// </summary>
    /// <remarks>
    /// This method can be used to minimize a collection's memory overhead 
    /// if no new elements will be added to the collection. 
    /// <para>
    /// The cost of reallocating and copying a large <see cref="IDeque{T}"/> can be considerable, however,
    /// so the <see cref="IDeque{T}.TrimExcess"/> method does nothing if the deque is at more than 90 percent of capacity.
    /// This avoids incurring a large reallocation cost for a relatively small gain.
    /// </para>
    /// <para>
    /// This method is an O(n) operation, where n is <see cref="ICountable.Count"/>.
    /// </para>
    /// To reset a <see cref="IDeque{T}"/> to its initial state, call the <see cref="IList.Clear"/> method
    /// before calling <see cref="IDeque{T}.TrimExcess"/> method. 
    /// Trimming an empty <see cref="IDeque{T}"/> sets the capacity of the <see cref="IDeque{T}"/>
    /// to the default capacity.
    /// </remarks>
    void TrimExcess();

    /// <summary>
    /// Gets head element in the <see cref="IDeque{T}"/>.
    /// </summary>
    T Head { get; }

    /// <summary>
    /// Gets head element in the <see cref="IDeque{T}"/>, or <see langword="default(T)" />, if deque is empty.
    /// </summary>
    T HeadOrDefault { get; }

    /// <summary>
    /// Gets tail element in the <see cref="IDeque{T}"/>.
    /// </summary>
    T Tail { get; }

    /// <summary>
    /// Gets tail element in the <see cref="IDeque{T}"/>, or <see langword="default(T)" />, if deque is empty.
    /// </summary>
    T TailOrDefault { get; }

    /// <summary>
    /// Adds <paramref name="element"/> to the <see cref="IDeque{T}"/> head.
    /// </summary>
    /// <param name="element">
    /// The element to add to the <see cref="IDeque{T}"/>.
    /// The value can be a <see langword="null"/> for reference types.
    /// </param>
    /// <remarks>
    /// <para>
    /// If <see cref="ICountable.Count"/> already equals the <see cref="IDeque{T}.Capacity"/>,
    /// the <see cref="IDeque{T}.Capacity"/> of the <see cref="IDeque{T}"/> is increased by 
    /// automatically reallocating the internal array, and the existing elements 
    /// are copied to the new array before the new element is added.
    /// </para>
    /// <para>
    /// If <see cref="ICountable.Count"/> is less than the <see cref="IDeque{T}.Capacity"/> of the internal array,
    /// this method is an O(1) operation. 
    /// If the internal array needs to be reallocated to accommodate the new element,
    /// this method becomes an O(n) operation, where n is <see cref="ICountable.Count"/>.
    /// </para>
    /// </remarks>
    void AddHead(T element);

    /// <summary>
    /// Adds <paramref name="element"/> to the <see cref="IDeque{T}"/> tail.
    /// </summary>
    /// <param name="element">
    /// The element to add to the <see cref="IDeque{T}"/>.
    /// The value can be a <see langword="null"/> for reference types.
    /// </param>
    /// <remarks>
    /// <para>
    /// If <see cref="ICountable.Count"/> already equals the <see cref="IDeque{T}.Capacity"/>,
    /// the <see cref="IDeque{T}.Capacity"/> of the <see cref="IDeque{T}"/> is increased by 
    /// automatically reallocating the internal array, and the existing elements 
    /// are copied to the new array before the new element is added.
    /// </para>
    /// <para>
    /// If <see cref="ICountable.Count"/> is less than the <see cref="IDeque{T}.Capacity"/> of the internal array,
    /// this method is an O(1) operation. 
    /// If the internal array needs to be reallocated to accommodate the new element,
    /// this method becomes an O(n) operation, where n is <see cref="ICountable.Count"/>.
    /// </para>
    /// </remarks>
    void AddTail(T element);

    /// <summary>
    /// Removes and returns the element at the head of the <see cref="IDeque{T}"/>.
    /// </summary>
    /// <returns>The element that is removed from the head of the <see cref="IDeque{T}"/>.</returns>
    /// <exception cref="InvalidOperationException">The <see cref="IDeque{T}"/> is empty.</exception>
    /// <remarks>
    /// This method is an O(1) operation.
    /// </remarks>
    T ExtractHead();

    /// <summary>
    /// Removes and returns the element at the tail of the <see cref="IDeque{T}"/>.
    /// </summary>
    /// <returns>The element that is removed from the tail of the <see cref="IDeque{T}"/>.</returns>
    /// <exception cref="InvalidOperationException">The <see cref="IDeque{T}"/> is empty.</exception>
    /// <remarks>
    /// This method is an O(1) operation.
    /// </remarks>
    T ExtractTail();

    /// <summary>
    /// Removes a range of items at the given index in the Deque. All items at indexes 
    /// greater than <paramref name="index"/> move down <paramref name="count"/> indices
    /// in the Deque.
    /// </summary>
    /// <param name="index">The index in the list to remove the range at. The
    /// first item in the list has index 0.</param>
    /// <param name="count">The number of items to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is
    /// less than zero or greater than or equal to Count, or <paramref name="count"/> is less than zero
    /// or too large.</exception>
    void RemoveRange(int index, int count);
  }
}
// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.04.02

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core.Comparison;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Collections
{
  /// <summary>
  /// Represents a variable size last-in-first-out (LIFO) collection of instances of the same arbitrary type.
  /// </summary>
  /// <typeparam name="TItem">The type of <see cref="Stack{T}"/> item.</typeparam>
  [Serializable]
  public class Stack<TItem>: IEnumerable<TItem>
  {
    private TItem[] items;
    private int size;

    /// <summary>
    /// Gets the <typeparamref name="TItem"/> at the specified index.
    /// </summary>
    /// <value>The <typeparamref name="TItem"/> instance.</value>
    public TItem this[int index]
    {
      get
      {
        ArgumentValidator.EnsureArgumentIsInRange(index, 0, size, "index");
        return items[index];
      }
    }

    /// <summary>
    /// Gets the number of elements contained in the <see cref="Stack{T}"/>.
    /// </summary>
    /// <value>The count.</value>
    public int Count
    {
      get { return size; }
    }

    /// <summary>
    /// Removes all objects from the <see cref="Stack{T}"/>.
    /// </summary>
    public void Clear()
    {
      Array.Clear(items, 0, size);
      size = 0;
    }

    /// <summary>
    /// Determines whether an element is in the <see cref="Stack{T}"/>.
    /// </summary>
    /// <param name="item">The item to locate in the <see cref="Stack{T}"/>.</param>
    /// <returns>
    ///   <see langword="true"/> if item is found in the <see cref="Stack{T}"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(TItem item)
    {
      int index = size;
      AdvancedComparer<TItem> comparer = AdvancedComparer<TItem>.Default;
      while (index-- > 0) {
        if (item == null) {
          if (items[index] == null) {
            return true;
          }
        }
        else if ((items[index] != null) && comparer.Equals(items[index], item)) {
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Copies the <see cref="Stack{T}"/> to an existing one-dimensional <see cref="Array"/>, starting at the specified array index.
    /// </summary>
    /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the elements copied from <see cref="Stack{T}"/>. The <see cref="Array"/> must have zero-based indexing.</param>
    /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
    public void CopyTo(TItem[] array, int arrayIndex)
    {
      ArgumentValidator.EnsureArgumentNotNull(array, "array");
      ArgumentValidator.EnsureArgumentIsInRange(arrayIndex, 0, array.Length, "arrayIndex");
      if ((array.Length - arrayIndex) < size)
        throw new ArgumentException(
          "The number of elements in the source Stack is greater than the available space from index to the end of the destination array");

      Array.Copy(items, 0, array, arrayIndex, size);
      Array.Reverse(array, arrayIndex, size);
    }

    /// <inheritdoc/>
    IEnumerator<TItem> IEnumerable<TItem>.GetEnumerator()
    {
      for (int index = 0; index < size; index++)
        yield return items[index];
    }

    /// <inheritdoc/>
    public IEnumerator GetEnumerator()
    {
      return ((IEnumerable<TItem>)this).GetEnumerator();
    }

    /// <summary>
    /// Returns the object at the top of the <see cref="Stack{T}"/> without removing it.
    /// </summary>
    /// <returns>The instance of <typeparamref name="TItem"/> at the top of the <see cref="Stack{T}"/>.</returns>
    public TItem Peek()
    {
      if (size == 0)
        throw new InvalidOperationException("The stack is empty.");

      return items[size - 1];
    }

    /// <summary>
    /// Removes and returns the object at the top of the <see cref="Stack{T}"/>
    /// </summary>
    /// <returns>The instance of <typeparamref name="TItem"/> removed from the top of the <see cref="Stack{T}"/>.</returns>
    public TItem Pop()
    {
      if (size == 0)
        throw new InvalidOperationException("The stack is empty.");
      TItem local = items[--size];
      items[size] = default(TItem);
      return local;
    }

    /// <summary>
    /// Inserts an object at the top of the <see cref="Stack{T}"/>.
    /// </summary>
    /// <param name="item">The item to push onto the <see cref="Stack{T}"/>.</param>
    public void Push(TItem item)
    {
      if (size == items.Length) {
        TItem[] destinationArray = new TItem[(items.Length == 0) ? 4 : (2*items.Length)];
        Array.Copy(items, 0, destinationArray, 0, size);
        items = destinationArray;
      }
      items[size++] = item;
    }

    /// <summary>
    /// Copies the <see cref="Stack{T}"/> to a new array.
    /// </summary>
    /// <returns>A new array containing copies of the elements of the <see cref="Stack{T}"/>.</returns>
    public TItem[] ToArray()
    {
      TItem[] localArray = new TItem[size];
      for (int i = 0; i < size; i++) {
        localArray[i] = items[(size - i) - 1];
      }
      return localArray;
    }

    /// <summary>
    /// Sets the capacity to the actual number of elements in the <see cref="Stack{T}"/>, if that number is less than 90 percent of current capacity.
    /// </summary>
    public void TrimExcess()
    {
      int num = (int)(items.Length*0.9);
      if (size < num) {
        TItem[] destinationArray = new TItem[size];
        Array.Copy(items, 0, destinationArray, 0, size);
        items = destinationArray;
      }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public Stack() : this(0)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate()" copy="true"/>
    /// </summary>
    /// <param name="capacity">The initial number of elements that the <see cref="Stack{TItem}"/> can contain.</param>
    public Stack(int capacity)
    {
      if (capacity < 0)
        throw new ArgumentOutOfRangeException("capacity", capacity, "Capacity is less then zero.");

      items = new TItem[capacity];
      size = 0;
    }
  }
}
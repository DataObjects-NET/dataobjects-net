// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.10.20

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Collections
{
  /// <summary>
  /// Defines a fixed stack-like list with three items.
  /// </summary>
  /// <typeparam name="T">Type of items.</typeparam>
  [Serializable]
  [DebuggerDisplay("Count = {Count}")]
  public struct FixedList3<T>
  {
    private T slot1;
    private T slot2;
    private T slot3;
    private int count;

    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="index">Index of the item</param>
    /// <exception cref="ArgumentOutOfRangeException">The index is greater or equal count of items.</exception>
    public T this[int index]
    {
      get
      {
        ArgumentValidator.EnsureArgumentIsInRange(index, 0, count-1, "index");
        switch (index) {
          case 0:
            return slot1;
          case 1:
            return slot2;
          default:
            return slot3;
        }
      }
      set
      {
        ArgumentValidator.EnsureArgumentIsInRange(index, 0, count-1, "index");
        switch (index) {
          case 0:
            slot1 = value;
            break;
          case 1:
            slot2 = value;
            break;
          default:
            slot3 = value;
            break;
        }
      }
    }

    /// <summary>
    /// Enumerates all items.
    /// </summary>
    public IEnumerable<T> Items
    {
      get
      {
        if (count > 0) {
          yield return slot1;
          if (count > 1){
            yield return slot2;
            if (count > 2)
              yield return slot3;
          }
        }
      }
    }

    /// <summary>
    /// Gets count of items.
    /// </summary>
    public int Count
    {
      [DebuggerStepThrough]
      get { return count; }
    }

    /// <summary>
    /// Adds item to the <see cref="FixedList3{T}"/> list.
    /// </summary>
    /// <param name="item">Item to add.</param>
    /// <exception cref="ArgumentException">The list already have three items.</exception>
    public void Push(T item)
    {
      ArgumentValidator.EnsureArgumentIsInRange(count, 0, 2, "count");
      switch (count) {
        case 0:
          slot1 = item;
          break;
        case 1:
          slot2 = item;
          break;
        case 2:
          slot3 = item;
          break;
      }
      count++;
    }

    /// <summary>
    /// Removes latest item from the <see cref="FixedList3{T}"/>.
    /// </summary>
    public T Pop()
    {
      T result = default(T);
      switch (count) {
        case 1:
          result = slot1;
          slot1 = default(T);
          count--;
          break;
        case 2:
          result = slot2;
          slot2 = default(T);
          count--;
          break;
        case 3:
          result = slot3;
          slot3 = default(T);
          count--;
          break;
      }
      return result;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="item">Item to add.</param>
    public FixedList3(T item)
    {
      slot1 = item;
      slot2 = default(T);
      slot3 = default(T);
      count = 1;
    }

    
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="first">First item to add.</param>
    /// <param name="second">Second item ot add.</param>
    public FixedList3(T first, T second)
    {
      slot1 = first;
      slot2 = second;
      slot3 = default(T);
      count = 2;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="first">First item to add.</param>
    /// <param name="second">Second item ot add.</param>
    /// <param name="third">Third item to add.</param>
    public FixedList3(T first, T second, T third)
    {
      slot1 = first;
      slot2 = second;
      slot3 = third;
      count = 3;
    }

  }
}

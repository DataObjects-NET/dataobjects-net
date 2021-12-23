// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.10.20

using System;
using System.Diagnostics;
using Xtensive.Core;


namespace Xtensive.Collections
{
  /// <summary>
  /// Defines a fixed stack-like list with three items.
  /// </summary>
  /// <typeparam name="T">Type of items.</typeparam>
  [Serializable]
  [DebuggerDisplay("Count = {Count}")]
  internal readonly struct FixedReadOnlyList3<T>
  {
    private readonly (T n1, T n2, T n3) slots;
    private readonly int count;

    /// <summary>
    /// Gets count of items.
    /// </summary>
    public int Count => count;

    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="index">Index of the item</param>
    /// <exception cref="ArgumentOutOfRangeException">The index is greater or equal count of items.</exception>
    public T this[int index]
    {
      get {
        if (index < 0 || index >= count) {
          ArgumentValidator.EnsureArgumentIsInRange(index, 0, count - 1, nameof(index));
        }
        return index switch {
          0 => slots.n1,
          1 => slots.n2,
          _ => slots.n3
        };
      }
    }


    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="item">Item to add.</param>
    public FixedReadOnlyList3(T item)
    {
      slots = (item, default, default);
      count = 1;
    }


    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="first">First item to add.</param>
    /// <param name="second">Second item ot add.</param>
    public FixedReadOnlyList3(T first, T second)
    {
      slots = (first, second, default);
      count = 2;
    }

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="first">First item to add.</param>
    /// <param name="second">Second item ot add.</param>
    /// <param name="third">Third item to add.</param>
    public FixedReadOnlyList3(T first, T second, T third)
    {
      slots = (first, second, third);
      count = 3;
    }
  }
}

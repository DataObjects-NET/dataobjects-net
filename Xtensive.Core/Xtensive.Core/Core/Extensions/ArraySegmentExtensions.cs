// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.27

using System;
using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Core
{
  /// <summary>
  /// <see cref="ArraySegment{T}"/> related extension methods.
  /// </summary>
  public static class ArraySegmentExtensions
  {
    /// <summary>
    /// Gets the element of <see cref="ArraySegment{T}"/> by its index.
    /// </summary>
    /// <typeparam name="T">The type of array element.</typeparam>
    /// <param name="segment">The array segment.</param>
    /// <param name="index">The index of item.</param>
    /// <returns>The item from the mapped array segment.</returns>
    public static T GetItem<T>(this ArraySegment<T> segment, int index)
    {
      ArgumentValidator.EnsureArgumentNotNull(segment, "segment");
      ArgumentValidator.EnsureArgumentIsInRange(index, 0, segment.Count - 1, "index");
      return segment.Array[segment.Offset + index];
    }

    /// <summary>
    /// Sets the element of <see cref="ArraySegment{T}"/> by its index.
    /// </summary>
    /// <typeparam name="T">The type of array element.</typeparam>
    /// <param name="segment">The array segment.</param>
    /// <param name="index">The index of the item to set.</param>
    public static void SetItem<T>(this ArraySegment<T> segment, int index, T value)
    {
      ArgumentValidator.EnsureArgumentNotNull(segment, "segment");
      ArgumentValidator.EnsureArgumentIsInRange(index, 0, segment.Count - 1, "index");
      segment.Array[segment.Offset + index] = value;
    }

    /// <summary>
    /// Converts <see cref="ArraySegment{T}"/> to <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of array element.</typeparam>
    /// <param name="segment">The array segment to convert to a sequence.</param>
    /// <returns>A sequence enumerating all the items from the array segment.</returns>
    public static IEnumerable<T> AsEnumerable<T>(this ArraySegment<T> segment)
    {
      ArgumentValidator.EnsureArgumentNotNull(segment, "segment");
      int lastPosition = segment.Offset + segment.Count;
      for (int i = segment.Offset; i < lastPosition; i++)
        yield return segment.Array[i];
    }
  }
}
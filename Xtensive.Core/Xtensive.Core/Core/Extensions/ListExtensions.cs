// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Anton U. Rogozhin
// Reimplemented by: Dmitri Maximov
// Created:    2007.07.04

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Resources;

namespace Xtensive.Core
{
  /// <summary>
  /// <see cref="ICollection{T}"/> related extension methods.
  /// </summary>
  public static class ListExtensions
  {
    #region Copy methods

    /// <summary>
    /// Copies the items from <paramref name="source"/> collection
    /// to <paramref name="target"/> array starting from specified
    /// <paramref name="targetIndex"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of item.</typeparam>
    /// <param name="source">Source collection to copy from.</param>
    /// <param name="target">Target array to copy to.</param>
    /// <param name="targetIndex">Index in <paramref name="target"/> array to start from.</param>
    /// <exception cref="ArgumentOutOfRangeException"><c>targetIndex</c> is out of range.</exception>
    /// <exception cref="ArgumentException">Destination array is too small.</exception>
    public static void Copy<TItem>(this IList<TItem> source, TItem[] target, int targetIndex)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      if (targetIndex < 0 || targetIndex > target.Length)
        throw new ArgumentOutOfRangeException("targetIndex");
      if ((target.Length - targetIndex) < source.Count)
        throw new ArgumentException(Strings.ExDestionationArrayIsTooSmall, "target");

      int count = source.Count;
      for (int i = 0; i < count; i++)
        target[targetIndex++] = source[i];
    }

    /// <summary>
    /// Copies the items from <paramref name="source"/> collection
    /// to <paramref name="target"/> array starting from specified
    /// <paramref name="targetIndex"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of item.</typeparam>
    /// <param name="source">Source collection to copy from.</param>
    /// <param name="target">Target array to copy to.</param>
    /// <param name="targetIndex">Index in <paramref name="target"/> array to start from.</param>
    /// <exception cref="ArgumentOutOfRangeException"><c>targetIndex</c> is out of range.</exception>
    /// <exception cref="ArgumentException">Destination array is too small or multidimensional.</exception>
    public static void Copy<TItem>(this IList<TItem> source, Array target, int targetIndex)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      if (targetIndex < 0 || targetIndex > target.Length)
        throw new ArgumentOutOfRangeException("targetIndex");
      if ((target.Length - targetIndex) < source.Count)
        throw new ArgumentException(Strings.ExDestionationArrayIsTooSmall, "target");
      if (target.Rank != 1)
        throw new ArgumentException(Strings.ExArrayIsMultidimensional, "target");
//      if (target.GetType().GetElementType().IsAssignableFrom(typeof(T)))
//        throw new ArgumentException(Strings.ExIncompatibleArrayType, "target");

      int count = source.Count;
      for (int i = 0; i < count; i++)
        target.SetValue(source[i], targetIndex++);
    }

    #endregion

    #region Reverse methods

    /// <summary>
    /// Returns an enumerable enumerating specified <paramref name="list"/> in backward direction.
    /// </summary>
    /// <typeparam name="TItem">The type of item.</typeparam>
    /// <param name="list">The list to enumerate in backward direction.</param>
    /// <returns>Enumerable enumerating specified <paramref name="list"/> in backward direction.</returns>
    public static IEnumerable<TItem> Reverse<TItem>(this IList<TItem> list)
    {
      ArgumentValidator.EnsureArgumentNotNull(list, "list");
      for (int i = list.Count-1; i>=0; i--)
        yield return list[i];
    }

    #endregion

    #region Segment method

    /// <summary>
    /// Enumerates segment of a list.
    /// </summary>
    /// <typeparam name="TItem">The type of the list item.</typeparam>
    /// <param name="items">The list to enumerate the segment of.</param>
    /// <param name="offset">Segment offset.</param>
    /// <param name="length">Segment length.</param>
    /// <returns>An enumerable iterating through the segment.</returns>
    public static IEnumerable<TItem> Segment<TItem>(this IList<TItem> items, int offset, int length)
    {
      ArgumentValidator.EnsureArgumentNotNull(items, "items");
      int lastIndex = offset + length;
      if (offset<0)
        offset = 0;
      if (lastIndex>items.Count)
        lastIndex = items.Count;
      for (int i = offset; i < lastIndex; i++)
        yield return items[i];
    }

    #endregion

    /// <summary>
    /// Ensures <paramref name="index"/> is in range of <paramref name="list"/> indexes.
    /// </summary>
    /// <param name="list">List to use the index range of.</param>
    /// <param name="index">Index value to check.</param>
    /// <exception cref="IndexOutOfRangeException">Specified index is not valid for the specified list.</exception>
    public static void EnsureIndexIsValid<T>(this IList<T> list, int index)
    {
      if (index < 0 || index >= list.Count)
        throw new IndexOutOfRangeException(Strings.ExIndexOutOfRange);
    }

    /// <summary>
    /// Ensures <paramref name="index"/> is in range of <paramref name="list"/> indexes.
    /// </summary>
    /// <param name="list">List to use the index range of.</param>
    /// <param name="index">Index value to check.</param>
    /// <exception cref="IndexOutOfRangeException">Specified index is not valid for the specified list.</exception>
    public static void EnsureIndexIsValid(this IList list, int index)
    {
      if (index < 0 || index >= list.Count)
        throw new IndexOutOfRangeException(Strings.ExIndexOutOfRange);
    }
  }
}
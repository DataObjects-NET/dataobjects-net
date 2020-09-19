// Copyright (C) 2007-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Anton U. Rogozhin
// Reimplemented by: Dmitri Maximov
// Created:    2007.07.04

using System;
using System.Collections.Generic;

namespace Xtensive.Core
{
  /// <summary>
  /// <see cref="ICollection{T}"/> related extension methods.
  /// </summary>
  public static class CollectionExtensionsEx
  {
    /// <summary>
    /// Converts <paramref name="source"/> collection to an array.
    /// </summary>
    /// <typeparam name="TItem">The type of collection items.</typeparam>
    /// <param name="source">Collection to convert.</param>
    /// <returns>An array containing all the items from the <paramref name="source"/>.</returns>
    public static TItem[] ToArray<TItem>(this ICollection<TItem> source)
    {
      var items = new TItem[source.Count];
      source.Copy(items, 0);
      return items;
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
    /// <exception cref="ArgumentException">Destination array is too small.</exception>
    public static void Copy<TItem>(this ICollection<TItem> source, TItem[] target, int targetIndex)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      if (targetIndex < 0 || targetIndex > target.Length)
        throw new ArgumentOutOfRangeException("targetIndex");
      if ((target.Length - targetIndex) < source.Count)
        throw new ArgumentException(Strings.ExDestionationArrayIsTooSmall, "target");

      foreach (TItem item in source)
        target[targetIndex++] = item;
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
    public static void Copy<TItem>(this ICollection<TItem> source, Array target, int targetIndex)
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

      foreach (TItem item in source)
        target.SetValue(item, targetIndex++);
    }

    /// <summary>
    /// Determines whether the specified <paramref name="collection"/> contains 
    /// none of items in specified set of <paramref name="items"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of item.</typeparam>
    /// <param name="collection">The collection.</param>
    /// <param name="items">The items to check for containment.</param>
    /// <returns>
    ///   <see langword="True"/> if the specified <paramref name="collection"/> none 
    ///   of items in specified set of <paramref name="items"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool ContainsNone<TItem>(this ICollection<TItem> collection, IEnumerable<TItem> items)
    {
      foreach (TItem item in items)
        if (collection.Contains(item))
          return false;
      return true;
    }

    /// <summary>
    /// Determines whether the specified <paramref name="collection"/> contains 
    /// all of items in specified set of <paramref name="items"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of item.</typeparam>
    /// <param name="collection">The collection.</param>
    /// <param name="items">The items to check for containment.</param>
    /// <returns>
    /// <see langword="True"/> if the specified <paramref name="collection"/> all
    /// of items in specified set of <paramref name="items"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool ContainsAll<TItem>(this ICollection<TItem> collection, IEnumerable<TItem> items)
    {
      foreach (TItem item in items)
        if (!collection.Contains(item))
          return false;
      return true;
    }

    /// <summary>
    /// Determines whether the specified <paramref name="collection"/> contains 
    /// any of items in specified set of <paramref name="items"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of item.</typeparam>
    /// <param name="collection">The collection.</param>
    /// <param name="items">The items to check for containment.</param>
    /// <returns>
    /// <see langword="True"/> if the specified <paramref name="collection"/> any 
    ///  of items in specified set of <paramref name="items"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool ContainsAny<TItem>(this ICollection<TItem> collection, IEnumerable<TItem> items)
    {
      foreach (TItem item in items)
        if (collection.Contains(item))
          return true;
      return false;
    }
  }
}
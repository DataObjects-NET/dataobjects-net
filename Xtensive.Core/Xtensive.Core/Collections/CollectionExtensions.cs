// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Anton U. Rogozhin
// Reimplemented by: Dmitri Maximov
// Created:    2007.07.04

using System;
using System.Collections.Generic;
using Xtensive.Core.Resources;
using System.Linq;

namespace Xtensive.Core.Collections
{
  /// <summary>
  /// <see cref="ICollection{T}"/> related extension methods.
  /// </summary>
  public static class CollectionExtensions
  {
    /// <summary>
    /// Converts <paramref name="source"/> collection to an array.
    /// </summary>
    /// <typeparam name="TItem">The type of collection items.</typeparam>
    /// <param name="source">Collection to convert.</param>
    /// <returns>An array containing all the items from the <paramref name="source"/>.</returns>
    public static TItem[] ToArray<TItem>(this ICollection<TItem> source)
    {
      TItem[] items = new TItem[source.Count];
      source.Copy(items, 0);
      return items;
    }

    /// <summary>
    /// Converts <paramref name="source"/> collection to an array with type case.
    /// </summary>
    /// <typeparam name="TItem">The type of collection items.</typeparam>
    /// <typeparam name="TNewItem">The type of array (result) items.</typeparam>
    /// <param name="source">Collection to convert.</param>
    /// <returns>An array containing all the items from the <paramref name="source"/>.</returns>
    public static TNewItem[] ToArray<TItem, TNewItem>(this ICollection<TItem> source)
      where TNewItem: TItem
    {
      TNewItem[] items = new TNewItem[source.Count];
      int i = 0;
      foreach (TItem item in source)
        items[i++] = (TNewItem)item;
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

    /// <summary>
    /// Unions the specified <see cref="IDictionary{TKey,TValue}"/> with <paramref name="right"/> enumerable int new <see cref="IDictionary{TKey,TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="left">The left dictionary.</param>
    /// <param name="right">The right enumerable.</param>
    public static IDictionary<TKey,TValue> Union<TKey,TValue>(this IDictionary<TKey,TValue> left, IEnumerable<KeyValuePair<TKey, TValue>> right)
    {
      var result = new Dictionary<TKey, TValue>(left);
      foreach (var pair in right)
        if (!result.ContainsKey(pair.Key))
          result.Add(pair.Key, pair.Value);
      return result;
    }
  }
}
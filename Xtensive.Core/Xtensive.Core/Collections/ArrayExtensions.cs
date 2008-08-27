// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Anton U. Rogozhin
// Reimplemented by: Dmitri Maximov
// Created:    2007.07.04

using System;
using System.Collections.Generic;
using Xtensive.Core.Comparison;
using Xtensive.Core.Reflection;
using Xtensive.Core.Resources;
using Xtensive.Core.Collections;

namespace Xtensive.Core.Collections
{
  /// <summary>
  /// <see cref="Array"/> related extension methods.
  /// </summary>
  public static class ArrayExtensions
  {
    /// <summary>
    /// Clones the array.
    /// </summary>
    /// <typeparam name="TItem">The type of array items.</typeparam>
    /// <param name="source">Array to clone.</param>
    /// <returns>An array containing all the items from the <paramref name="source"/>.</returns>
    public static TItem[] Copy<TItem>(this TItem[] source)
    {
      TItem[] items = new TItem[source.Length];
      source.Copy(items, 0);
      return items;
    }

    /// <summary>
    /// Clones <paramref name="source"/> array with type case.
    /// </summary>
    /// <typeparam name="TItem">The type of source array items.</typeparam>
    /// <typeparam name="TNewItem">The type of result array items.</typeparam>
    /// <param name="source">Collection to convert.</param>
    /// <returns>An array containing all the items from the <paramref name="source"/>.</returns>
    public static TNewItem[] Cast<TItem, TNewItem>(this TItem[] source)
      where TNewItem: TItem
    {
      TNewItem[] items = new TNewItem[source.Length];
      int i = 0;
      foreach (TItem item in source)
        items[i++] = (TNewItem)item;
      return items;
    }

    /// <summary>
    /// Clones <paramref name="source"/> array with element conversion.
    /// </summary>
    /// <typeparam name="TItem">The type of item.</typeparam>
    /// <typeparam name="TNewItem">The type of item to convert to.</typeparam>
    /// <param name="source">The array to convert.</param>
    /// <param name="converter">A delegate that converts each element.</param>
    /// <returns>An array of converted elements.</returns>
    public static TNewItem[] Convert<TItem, TNewItem>(this TItem[] source, Converter<TItem, TNewItem> converter)
    {
      ArgumentValidator.EnsureArgumentNotNull(converter, "converter");
      TNewItem[] items = new TNewItem[source.Length];
      int i = 0;
      foreach (TItem item in source)
        items[i++] = converter(item);
      return items;
    }

    /// <summary>
    /// Copies the items from <paramref name="source"/> array
    /// to <paramref name="target"/> starting from specified
    /// <paramref name="targetIndex"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of item.</typeparam>
    /// <param name="source">Source array to copy from.</param>
    /// <param name="target">Target array to copy to.</param>
    /// <param name="targetIndex">Index in <paramref name="target"/> array to start from.</param>
    public static void Copy<TItem>(this TItem[] source, TItem[] target, int targetIndex)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      if (targetIndex < 0 || targetIndex > target.Length)
        throw new ArgumentOutOfRangeException("targetIndex");
      if ((target.Length - targetIndex) < source.Length)
        throw new ArgumentException(Strings.ExDestionationArrayIsTooSmall, "target");

      source.CopyTo(target, targetIndex);
//      int length = source.Length;
//      for (int i = 0; i < length; i++)
//        target[targetIndex++] = source[i];
    }

    /// <summary>
    /// Gets the index of first occurrence of the <paramref name="item"/>
    /// in the <paramref name="items"/> array, if found;
    /// otherwise returns <see langword="-1"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of item.</typeparam>
    /// <param name="items">Array to search for the item.</param>
    /// <param name="item">Item to locate in the array.</param>
    /// <returns>
    /// Index of first occurrence of the <paramref name="item"/>
    /// in the <paramref name="items"/> array, if found;
    /// otherwise, <see langword="-1"/>.
    /// </returns>
    public static int IndexOf<TItem>(this TItem[] items, TItem item) 
    {
      ArgumentValidator.EnsureArgumentNotNull(items, "items");
      for (int i = 0; i < items.Length; i++)
        if (AdvancedComparerStruct<TItem>.System.Equals(item, items[i]))
          return i;
      return -1;
    }

    /// <summary>
    /// Gets the index of first occurrence of the <paramref name="item"/>
    /// in the <paramref name="items"/> array, if found;
    /// otherwise returns <see langword="-1"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of item.</typeparam>
    /// <param name="items">Array to search for the item.</param>
    /// <param name="item">Item to locate in the array.</param>
    /// <param name="byReference">Indicates whether just references
    /// should be compared.</param>
    /// <returns>
    /// Index of first occurrence of the <paramref name="item"/>
    /// in the <paramref name="items"/> array, if found;
    /// otherwise, <see langword="-1"/>.
    /// </returns>
    public static int IndexOf<TItem>(this TItem[] items, TItem item, bool byReference) 
    {
      ArgumentValidator.EnsureArgumentNotNull(items, "items");
      if (!byReference)
        return IndexOf(items, item);
      if (typeof(TItem).IsValueType)
        throw new InvalidOperationException(string.Format(
          Strings.ExTypeXMustBeReferenceType, 
          typeof(TItem).GetShortName()));
      for (int i = 0; i < items.Length; i++)
        if (ReferenceEquals(item, items[i]))
          return i;
      return -1;
    }
  }
}
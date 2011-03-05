// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Anton U. Rogozhin
// Reimplemented by: Dmitri Maximov
// Created:    2007.07.04

using System;
using System.Collections.Generic;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Resources;

namespace Xtensive.Core
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
      var items = new TItem[source.Length];
      source.Copy(items, 0);
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
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="targetIndex"/> is out of range.</exception>
    /// <exception cref="ArgumentException"><paramref name="target"/> array is too small.</exception>
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
    /// Clones <paramref name="source"/> array with type case.
    /// </summary>
    /// <typeparam name="TItem">The type of source array items.</typeparam>
    /// <typeparam name="TNewItem">The type of result array items.</typeparam>
    /// <param name="source">Collection to convert.</param>
    /// <returns>An array containing all the items from the <paramref name="source"/>.</returns>
    public static TNewItem[] Cast<TItem, TNewItem>(this TItem[] source)
      where TNewItem: TItem
    {
      var items = new TNewItem[source.Length];
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
      var items = new TNewItem[source.Length];
      int i = 0;
      foreach (TItem item in source)
        items[i++] = converter(item);
      return items;
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
    /// Enumerates segment of an array.
    /// </summary>
    /// <typeparam name="TItem">The type of the array item.</typeparam>
    /// <param name="items">The array to enumerate the segment of.</param>
    /// <param name="offset">Segment offset.</param>
    /// <param name="length">Segment length.</param>
    /// <returns>An enumerable iterating through the segment.</returns>
    public static IEnumerable<TItem> Segment<TItem>(this TItem[] items, int offset, int length)
    {
      ArgumentValidator.EnsureArgumentNotNull(items, "items");
      int lastIndex = offset + length;
      if (offset<0)
        offset = 0;
      if (lastIndex>items.Length)
        lastIndex = items.Length;
      for (int i = offset; i < lastIndex; i++)
        yield return items[i];
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
    /// <exception cref="InvalidOperationException">Value type is passed instead of class.</exception>
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

    /// <summary>
    /// Selects the specified item from the ordered sequence of items
    /// produced by ordering the <paramref name="items"/>.
    /// The original sequence will be partially reordered!
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="items">The items to select from.</param>
    /// <param name="index">The offset of the item to select from the ordered sequence.</param>
    /// <returns>The specified item from the ordered sequence of items.</returns>
    public static TItem Select<TItem>(this TItem[] items, Func<TItem, TItem, int> comparer, int index)
    {
      var r = new Random();
      int leftIndex = 0;
      int rightIndex = items.Length - 1;
      while (true) {
        int pivotIndex = leftIndex + r.Next(rightIndex - leftIndex + 1);
        pivotIndex = items.Partition(comparer, leftIndex, rightIndex, pivotIndex);
        if (index==pivotIndex)
          return items[index];
        else if (index < pivotIndex)
          rightIndex = pivotIndex - 1;
        else
          leftIndex = pivotIndex + 1;
      }
    }

    /// <summary>
    /// Creates new array consisting of <paramref name="items"/>
    /// and <paramref name="item"/> added before array elements.
    /// If <paramref name="items"/> is <see langword="null"/>
    /// returns array that contains just <paramref name="item"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="items">The items.</param>
    /// <param name="item">The prefix item.</param>
    /// <returns>Result array.</returns>
    public static TItem[] Prepend<TItem>(this TItem[] items, TItem item)
    {
      if (items == null || items.Length == 0)
        return new [] {item};
      var result = new TItem[items.Length + 1];
      Array.Copy(items, 0, result, 1, items.Length);
      result[0] = item;
      return result;
    }

    /// <summary>
    /// Creates new array consisting of <paramref name="items"/>
    /// and <paramref name="item"/> added after array elements.
    /// If <paramref name="items"/> is <see langword="null"/>
    /// returns array that contains just <paramref name="item"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="items">The items.</param>
    /// <param name="item">The prefix item.</param>
    /// <returns>Result array.</returns>
    public static TItem[] Append<TItem>(this TItem[] items, TItem item)
    {
      if (items == null || items.Length == 0)
        return new [] {item};
      var result = new TItem[items.Length + 1];
      Array.Copy(items, result, items.Length);
      result[items.Length] = item;
      return result;
    }

    /// <summary>
    /// Combines the specified source and target arrays into new one.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    /// <returns></returns>
    public static TItem[] Combine<TItem>(this TItem[] source, TItem[] target)
    {
      if (source == null || source.Length == 0)
        return target;
      if (target == null || target.Length == 0)
        return source;

      var result = new TItem[source.Length + target.Length];
      Array.Copy(source, result, source.Length);
      Array.Copy(target, 0, result, source.Length, target.Length);

      return result;
    }

    private static int Partition<TItem>(this TItem[] items, Func<TItem, TItem, int> comparer, int leftIndex, int rightIndex, int pivotIndex)
    {
      var pivot = items[pivotIndex];
      // Swap
      var tmp = items[rightIndex];
      items[rightIndex] = pivot;
      items[pivotIndex] = tmp;
      // Loop
      int storeIndex = leftIndex;
      for (int i = leftIndex; i < rightIndex; i++) {
        if (comparer(items[i], pivot) < 0) {
          // Swap
          tmp = items[storeIndex];
          items[storeIndex] = items[i];
          items[i] = tmp;
          storeIndex++;
        }
      }
      // Swap
      tmp = items[rightIndex];
      items[rightIndex] = items[storeIndex];
      items[storeIndex] = tmp;
      return storeIndex;
    }
  }
}
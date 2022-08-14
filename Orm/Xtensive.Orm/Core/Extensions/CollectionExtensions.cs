// Copyright (C) 2007-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Anton U. Rogozhin
// Reimplemented by: Dmitri Maximov
// Created:    2007.07.04

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;

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

    /// <summary>Projects each element of a sequence into a new form.</summary>
    /// <param name="source">A collection of values to invoke a transform function on.</param>
    /// <param name="selector">A transform function to apply to each element.</param>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <typeparam name="TResult">The type of the value returned by <paramref name="selector" />.</typeparam>
    /// <returns>An <see cref="T:System.Array`1" /> whose elements are the result of invoking the transform function on each element of <paramref name="source" />.</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.</exception>
    public static TResult[] SelectToArray<TSource, TResult>(
      this ICollection<TSource> source, Func<TSource, TResult> selector)
    {
      return source.Select(selector).ToArray(source.Count);
    }

    /// <summary>Projects each element of a sequence into a new form by incorporating the element's index.</summary>
    /// <param name="source">A collection of values to invoke a transform function on.</param>
    /// <param name="selector">A transform function to apply to each source element; the second parameter of the function represents the index of the source element.</param>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <typeparam name="TResult">The type of the value returned by <paramref name="selector" />.</typeparam>
    /// <returns>An <see cref="T:System.Array`1" /> whose elements are the result of invoking the transform function on each element of <paramref name="source" />.</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.</exception>
    public static TResult[] SelectToArray<TSource, TResult>(
      this ICollection<TSource> source, Func<TSource, int, TResult> selector)
    {
      return source.Select(selector).ToArray(source.Count);
    }

    /// <summary>Projects each element of a sequence into a new form.</summary>
    /// <param name="source">A collection of values to invoke a transform function on.</param>
    /// <param name="selector">A transform function to apply to each element.</param>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <typeparam name="TResult">The type of the value returned by <paramref name="selector" />.</typeparam>
    /// <returns>An <see cref="T:System.Collections.Generic.List`1" /> whose elements are the result of invoking the transform function on each element of <paramref name="source" />.</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.</exception>
    public static List<TResult> SelectToList<TSource, TResult>(
      this ICollection<TSource> source, Func<TSource, TResult> selector)
    {
      return source.Select(selector).ToList(source.Count);
    }

    /// <summary>Projects each element of a sequence into a new form by incorporating the element's index.</summary>
    /// <param name="source">A collection of values to invoke a transform function on.</param>
    /// <param name="selector">A transform function to apply to each source element; the second parameter of the function represents the index of the source element.</param>
    /// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
    /// <typeparam name="TResult">The type of the value returned by <paramref name="selector" />.</typeparam>
    /// <returns>An <see cref="T:System.Collections.Generic.List`1" /> whose elements are the result of invoking the transform function on each element of <paramref name="source" />.</returns>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="source" /> or <paramref name="selector" /> is <see langword="null" />.</exception>
    public static List<TResult> SelectToList<TSource, TResult>(
      this ICollection<TSource> source, Func<TSource, int, TResult> selector)
    {
      return source.Select(selector).ToList(source.Count);
    }

    /// <summary>
    /// Projects each element of a sequence into two elements of new form and adds it to the first or the second array respectively.
    /// </summary>
    /// <typeparam name="TSource">Type of source sequence.</typeparam>
    /// <typeparam name="TFirst">Type of elements of first result array.</typeparam>
    /// <typeparam name="TSecond">Type of elements of first result array.</typeparam>
    /// <param name="source">A collection of values to invoke a transform functions on.</param>
    /// <param name="firstArraySelector">A transform function to apply to each element to get element of the first array.</param>
    /// <param name="secondArraySelector">A transform function to apply to each element to get element of the second array.</param>
    /// <returns>Pair of two arrays.</returns>
    internal static Pair<TFirst[], TSecond[]> SelectToArrays<TSource, TFirst,TSecond>(
      this ICollection<TSource> source, Func<TSource, TFirst> firstArraySelector, Func<TSource, TSecond> secondArraySelector)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(firstArraySelector, nameof(firstArraySelector));
      ArgumentValidator.EnsureArgumentNotNull(secondArraySelector, nameof(secondArraySelector));

      var first = new TFirst[source.Count];
      var second = new TSecond[source.Count];
      var index = 0;
      foreach (var item in source) {
        first[index] = firstArraySelector(item);
        second[index] = secondArraySelector(item);
        index++;
      }
      return new Pair<TFirst[], TSecond[]>(first, second);
    }
  }
}

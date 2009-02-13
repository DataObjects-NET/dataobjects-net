// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.05.16

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Core.Comparison;

namespace Xtensive.Core.Collections
{
  /// <summary>
  /// <see cref="IEnumerable{T}"/> related extension methods.
  /// </summary>
  public static class EnumerableExtensions
  {
    /// <summary>
    /// Calculates hash code of <see cref="IEnumerable"/> by XOR hash codes of it's items.
    /// </summary>
    ///<param name="items">Enumerable to calculate hash for.</param>
    /// <typeparam name="TItem">The type of item.</typeparam>
    ///<returns>Hash code, calculated by enumerable items. If enumerable is null or empty returns 0.</returns>
    public static int GetHashCodeRecursive<TItem>(this IEnumerable<TItem> items)
    {
      if (items==null) 
        return 0;
      return items.Aggregate(0, (previousValue, item) => previousValue ^ item.GetHashCode());
    }


    /// <summary>
    /// Indicates whether enumerable is empty or not 
    /// by attempting to cast it to <see cref="ICollection{T}"/>, <see cref="ICountable{TItem}"/> and <see cref="IQueryable{T}"/>.
    /// May return false negative response.
    /// </summary>
    /// <typeparam name="TItem">The type of item.</typeparam>
    /// <param name="items">Items to check.</param>
    /// <returns><see langword="True"/> if collection is definitely <see langword="null"/> or empty;
    /// otherwise, <see langword="false"/>.</returns>
    public static bool IsNullOrEmpty<TItem>(this IEnumerable<TItem> items)
    {
      if (items==null)
        return true;
      long? count = items.TryGetCount();
      if (!count.HasValue)
        return false;
      return count.GetValueOrDefault()==0;
    }

    /// <summary>
    /// Gets the count of items (as <see cref="long"/>) of <see cref="IEnumerable{T}"/>, if it is actually
    /// <see cref="ICollection{T}"/>, <see cref="ICountable{TItem}"/> or <see cref="IQueryable{T}"/>.
    /// Otherwise returns <see langword="null"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of item.</typeparam>
    /// <param name="items">Items to get count of.</param>
    /// <returns>The count of items, if it's possible to get it;
    /// otherwise, <see langword="null"/>.</returns>
    public static long? TryGetLongCount<TItem>(this IEnumerable<TItem> items)
    {
      {
        var c1 = items as ICollection<TItem>;
        if (c1 != null)
          return c1.Count;
      }
      {
        var c1 = items as ICountable<TItem>;
        if (c1 != null)
          return c1.Count;
      }
      {
        var c1 = items as IQueryable<TItem>;
        if (c1 != null)
          return c1.LongCount();
      }
      return null;
    }

    /// <summary>
    /// Gets the count of items of <see cref="IEnumerable{T}"/>, if it is actually
    /// <see cref="ICollection{T}"/>, <see cref="ICountable{TItem}"/> or <see cref="IQueryable{T}"/>.
    /// Otherwise returns <see langword="null"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of item.</typeparam>
    /// <param name="items">Items to get count of.</param>
    /// <returns>The count of items, if it's possible to get it;
    /// otherwise, <see langword="null"/>.</returns>
    public static int? TryGetCount<TItem>(this IEnumerable<TItem> items)
    {
      {
        var c1 = items as ICollection<TItem>;
        if (c1 != null)
          return c1.Count;
      }
      {
        var c1 = items as ICountable<TItem>;
        if (c1 != null)
          return (int)c1.Count;
      }
      {
        var c1 = items as IQueryable<TItem>;
        if (c1 != null)
          return (int)c1.LongCount();
      }
      return null;
    }

    /// <summary>
    /// Converts the elements of <paramref name="source"/> sequence 
    /// using specified <paramref name="converter"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of item.</typeparam>
    /// <typeparam name="TNewItem">The type of item to convert to.</typeparam>
    /// <param name="source">The sequence to convert.</param>
    /// <param name="converter">A delegate that converts each element.</param>
    /// <returns>A sequence of converted elements.</returns>
    public static IEnumerable<TNewItem> Convert<TItem, TNewItem>(this IEnumerable<TItem> source, Converter<TItem, TNewItem> converter)
    {
      ArgumentValidator.EnsureArgumentNotNull(converter, "converter");
      foreach (TItem item in source)
        yield return converter(item);
    }

    /// <summary>
    /// Converts the <paramref name="source"/> to comma-delimited string.
    /// </summary>
    /// <typeparam name="TItem">The type of item.</typeparam>
    /// <param name="source">The sequence to convert.</param>
    /// <returns>Comma-delimited string containing string representation 
    /// of all the items from <paramref name="source"/>.</returns>
    public static string ToCommaDelimitedString<TItem>(this IEnumerable<TItem> source)
    {
      return ToDelimitedString((IEnumerable) source, ", ");
    }

    /// <summary>
    /// Converts the <paramref name="source"/> to comma-delimited string.
    /// </summary>
    /// <param name="source">The sequence to convert.</param>
    /// <returns>Comma delimited string combining string representations
    /// of all the items from <paramref name="source"/>.</returns>
    public static string ToCommaDelimitedString(this IEnumerable source)
    {
      return source.ToDelimitedString(", ");
    }

    /// <summary>
    /// Converts the <paramref name="source"/> to a delimited string.
    /// </summary>
    /// <typeparam name="TItem">The type of item.</typeparam>
    /// <param name="source">The sequence to convert.</param>
    /// <param name="delimiter">The delimiter.</param>
    /// <returns>A delimited string combining string representations
    /// of all the items from <paramref name="source"/>.</returns>
    public static string ToDelimitedString<TItem>(this IEnumerable<TItem> source, string delimiter)
    {
      if (source==null)
        return string.Empty;
      StringBuilder sb = new StringBuilder();
      bool prefixWithComma = false;
      foreach (var item in source) {
        if (prefixWithComma)
          sb.Append(delimiter);
        sb.Append(item.ToString());
        prefixWithComma = true;
      }
      return sb.ToString();
    }

    /// <summary>
    /// Converts the <paramref name="source"/> to delimited string.
    /// </summary>
    /// <param name="source">The sequence to convert.</param>
    /// <param name="separator">The delimiter.</param>
    /// <returns>Delimited string containing string representation 
    /// of all the items from <paramref name="source"/>.</returns>
    public static string ToDelimitedString(this IEnumerable source, string separator)
    {
      if (source==null)
        return string.Empty;
      StringBuilder sb = new StringBuilder();
      bool prefixWithComma = false;
      foreach (object item in source) {
        if (prefixWithComma)
          sb.Append(separator);
        sb.Append(item.ToString());
        prefixWithComma = true;
      }
      return sb.ToString();
    }

    /// <summary>
    /// Determines whether this <see cref="IEnumerable{T}"/> equals to another, 
    /// i.e. contains the same items in the same order.
    /// </summary>
    /// <typeparam name="TItem">The type of item.</typeparam>
    /// <param name="items">This <see cref="IEnumerable"/>.</param>
    /// <param name="other">The <see cref="IEnumerable"/> to compare with.</param>
    /// <returns>
    /// <see langword="true"/> if this <see cref="IEnumerable{T}"/> equals to the specified <see cref="IEnumerable{T}"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool EqualsTo<TItem>(this IEnumerable<TItem> items, IEnumerable<TItem> other)
    {     
      long? thisCount = items.TryGetLongCount();
      if (thisCount.HasValue) {
        long? otherCount = other.TryGetCount();
        if (otherCount.HasValue && otherCount!=thisCount)
          return false;
      }           
      IEnumerator<TItem> enumerator = items.GetEnumerator();

      foreach (var item in other) {
        if (!enumerator.MoveNext() || !AdvancedComparerStruct<TItem>.System.Equals(enumerator.Current, item))
          return false;
      }
      return true;
    }

    /// <summary>
    /// Constructs <see cref="IEnumerable{T}"/> from
    /// this <see cref="IEnumerable{T}"/> and specified <see cref="IEnumerable{T}"/>
    /// by applying <paramref name="projector"/> for each pair of items.
    /// If one input <see cref="IEnumerable{T}"/> is short,
    /// excess elements of the longer <see cref="IEnumerable{T}"/> are discarded.
    /// </summary>
    /// <typeparam name="TLeft">Type of first <see cref="IEnumerable{T}"/></typeparam>
    /// <typeparam name="TRight">Type of second <see cref="IEnumerable{T}"/></typeparam>
    /// <typeparam name="TResult">Type of result</typeparam>
    /// <param name="leftSequence">First <see cref="IEnumerable{T}"/></param>
    /// <param name="rightSequence">Second <see cref="IEnumerable{T}"/></param>
    /// <param name="projector"></param>
    /// <returns>result of applying <paramref name="projector"/> for each pair of items.</returns>
    public static IEnumerable<TResult> ZipWith<TLeft,TRight,TResult>(this IEnumerable<TLeft> leftSequence, IEnumerable<TRight> rightSequence, Func<TLeft,TRight,TResult> projector)
    {
      ArgumentValidator.EnsureArgumentNotNull(leftSequence, "leftSequence");
      ArgumentValidator.EnsureArgumentNotNull(rightSequence, "rightSequence");
      ArgumentValidator.EnsureArgumentNotNull(projector, "projector");

      return ZipInternal(leftSequence, rightSequence, projector);
    }

    private static IEnumerable<TResult> ZipInternal<TLeft,TRight,TResult>(this IEnumerable<TLeft> leftSequence, IEnumerable<TRight> rightSequence, Func<TLeft,TRight,TResult> projector)
    {
      var leftEnum = leftSequence.GetEnumerator();
      var rightEnum = rightSequence.GetEnumerator();
      while (leftEnum.MoveNext() && rightEnum.MoveNext())
        yield return projector(leftEnum.Current, rightEnum.Current);
    }

    /// <summary>
    /// Gets the items from the segment.
    /// </summary>
    /// <param name="segment">The segment.</param>
    public static IEnumerable<int> GetItems(this Segment<int> segment)
    {
      return Enumerable.Range(segment.Offset, segment.Length);
    }

    /// <summary>
    /// Gets the items from the segment.
    /// </summary>
    /// <param name="segment">The segment.</param>
    /// <returns></returns>
    public static IEnumerable<long> GetItems(this Segment<long> segment)
    {
      for (long i = segment.Offset; i < segment.EndOffset; i++)
        yield return i;
    }

    /// <summary>
    /// Gets the items from the segment.
    /// </summary>
    /// <param name="segment">The segment.</param>
    /// <returns></returns>
    public static IEnumerable<short> GetItems(this Segment<short> segment)
    {
      for (short i = segment.Offset; i < segment.EndOffset; i++)
        yield return i;
    }
  }
}
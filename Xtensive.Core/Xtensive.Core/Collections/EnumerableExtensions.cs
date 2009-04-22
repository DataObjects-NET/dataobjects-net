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
    /// Applies the specified <paramref name="action"/> to all the items 
    /// from the <paramref name="items"/> sequence.
    /// </summary>
    /// <typeparam name="T">Type of the sequence item.</typeparam>
    /// <param name="items">The sequence to apply the <paramref name="action"/> to.</param>
    /// <param name="action">The action to apply.</param>
    public static void Apply<T>(this IEnumerable<T> items, Action<T, int> action)
    {
      int i = 0;
      foreach (var item in items)
        action.Invoke(item, i++);
    }

    /// <summary>
    /// Applies the specified <paramref name="action"/> to all the items 
    /// from the <paramref name="items"/> sequence.
    /// </summary>
    /// <typeparam name="T">Type of the sequence item.</typeparam>
    /// <param name="items">The sequence to apply the <paramref name="action"/> to.</param>
    /// <param name="action">The action to apply.</param>
    public static void Apply<T>(this IEnumerable<T> items, Action<T> action)
    {
      foreach (var item in items)
        action.Invoke(item);
    }

    /// <summary>
    /// Converts the sequence to the <see cref="HashSet{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of sequence item.</typeparam>
    /// <param name="source">The sequence to convert.</param>
    /// <returns>A new <see cref="HashSet{T}"/> instance containing 
    /// all the unique items from the <paramref name="source"/> sequence.</returns>
    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      return new HashSet<T>(source);
    }

    /// <summary>
    /// Converts the sequence to the <see cref="HashSet{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of sequence item.</typeparam>
    /// <param name="source">The sequence to convert.</param>
    /// <param name="ensureNoDuplicates">If set to <see langword="true"/>, an exception 
    /// will be thrown if there are duplicates;
    /// otherwise result will contain only unique items.</param>
    /// <returns>A new <see cref="HashSet{T}"/> instance containing 
    /// all the unique items from the <paramref name="source"/> sequence.</returns>
    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, bool ensureNoDuplicates)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      if (!ensureNoDuplicates)
        return new HashSet<T>(source);
      var hashSet = new HashSet<T>();
      foreach (var item in source)
        hashSet.AddOne(item);
      return hashSet;
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
        using (var e = items.GetEnumerator())
          return !e.MoveNext();
      return count.Value==0;
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
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(converter, "converter");
      foreach (TItem item in source)
        yield return converter(item);
    }

    /// <summary>
    /// Calculates hash code of <see cref="IEnumerable"/> by XOR hash codes of it's items.
    /// </summary>
    ///<param name="items">Enumerable to calculate hash for.</param>
    /// <typeparam name="TItem">The type of item.</typeparam>
    ///<returns>Hash code, calculated by enumerable items. If enumerable is null or empty returns 0.</returns>
    public static int CalculateHashCode<TItem>(this IEnumerable<TItem> items)
    {
      if (items==null) 
        return 0;
      return items.Aggregate(0, (previousValue, item) => previousValue ^ item.GetHashCode());
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
      var sb = new StringBuilder();
      bool insertDelimiter = false;
      foreach (var item in source) {
        if (insertDelimiter)
          sb.Append(delimiter);
        sb.Append(item.ToString());
        insertDelimiter = true;
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
      var sb = new StringBuilder();
      bool insertDelimiter = false;
      foreach (object item in source) {
        if (insertDelimiter)
          sb.Append(separator);
        sb.Append(item.ToString());
        insertDelimiter = true;
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

      using (IEnumerator<TItem> enumerator = items.GetEnumerator())
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
    /// <typeparam name="TLeft">Type of first <see cref="IEnumerable{T}"/>.</typeparam>
    /// <typeparam name="TRight">Type of second <see cref="IEnumerable{T}"/>.</typeparam>
    /// <typeparam name="TResult">Type of result.</typeparam>
    /// <param name="leftSequence">First <see cref="IEnumerable{T}"/>.</param>
    /// <param name="rightSequence">Second <see cref="IEnumerable{T}"/>.</param>
    /// <param name="projector">A delegate constructing <typeparamref name="TResult"/> from 
    /// <typeparamref name="TLeft"/> and <typeparamref name="TRight"/> values.</param>
    /// <returns>Result of applying <paramref name="projector"/> for each pair of items.</returns>
    public static IEnumerable<TResult> Zip<TLeft, TRight, TResult>(
      this IEnumerable<TLeft> leftSequence, IEnumerable<TRight> rightSequence, Func<TLeft, TRight, TResult> projector)
    {
      ArgumentValidator.EnsureArgumentNotNull(leftSequence, "leftSequence");
      ArgumentValidator.EnsureArgumentNotNull(rightSequence, "rightSequence");
      ArgumentValidator.EnsureArgumentNotNull(projector, "projector");

      using (var leftEnum = leftSequence.GetEnumerator())
      using (var rightEnum = rightSequence.GetEnumerator())
        while (leftEnum.MoveNext() && rightEnum.MoveNext())
          yield return projector(leftEnum.Current, rightEnum.Current);
    }

    /// <summary>
    /// Constructs <see cref="IEnumerable{T}"/> from
    /// this <see cref="IEnumerable{T}"/> and specified <see cref="IEnumerable{T}"/>
    /// by creating a <see cref="Pair{TFirst,TSecond}"/> from each pair of items.
    /// If one input <see cref="IEnumerable{T}"/> is short,
    /// excess elements of the longer <see cref="IEnumerable{T}"/> are discarded.
    /// </summary>
    /// <typeparam name="TLeft">Type of first <see cref="IEnumerable{T}"/>.</typeparam>
    /// <typeparam name="TRight">Type of second <see cref="IEnumerable{T}"/>.</typeparam>
    /// <param name="leftSequence">First <see cref="IEnumerable{T}"/>.</param>
    /// <param name="rightSequence">Second <see cref="IEnumerable{T}"/>.</param>
    /// <returns>Zip result.</returns>
    public static IEnumerable<Pair<TLeft,TRight>> Zip<TLeft, TRight>(
      this IEnumerable<TLeft> leftSequence, IEnumerable<TRight> rightSequence)
    {
      ArgumentValidator.EnsureArgumentNotNull(leftSequence, "leftSequence");
      ArgumentValidator.EnsureArgumentNotNull(rightSequence, "rightSequence");

      using (var leftEnum = leftSequence.GetEnumerator())
      using (var rightEnum = rightSequence.GetEnumerator())
        while (leftEnum.MoveNext() && rightEnum.MoveNext())
          yield return new Pair<TLeft, TRight>(leftEnum.Current, rightEnum.Current);
    }

    /// <summary>
    /// Constructs <see cref="IEnumerable{T}"/> from
    /// this <see cref="IEnumerable{T}"/> and specified <see cref="IEnumerable{T}"/>
    /// by applying <paramref name="projector"/> for each pair of items.
    /// If one input <see cref="IEnumerable{T}"/> is short, it is extended with default values.
    /// </summary>
    /// <typeparam name="TLeft">Type of first <see cref="IEnumerable{T}"/>.</typeparam>
    /// <typeparam name="TRight">Type of second <see cref="IEnumerable{T}"/>.</typeparam>
    /// <typeparam name="TResult">Type of result.</typeparam>
    /// <param name="leftSequence">First <see cref="IEnumerable{T}"/>.</param>
    /// <param name="rightSequence">Second <see cref="IEnumerable{T}"/>.</param>
    /// <param name="projector">A delegate constructing <typeparamref name="TResult"/> from 
    /// <typeparamref name="TLeft"/> and <typeparamref name="TRight"/> values.</param>
    /// <returns>Result of applying <paramref name="projector"/> for each pair of items.</returns>
    public static IEnumerable<TResult> ZipExtend<TLeft, TRight, TResult>(
      this IEnumerable<TLeft> leftSequence, IEnumerable<TRight> rightSequence, Func<TLeft, TRight, TResult> projector)
    {
      ArgumentValidator.EnsureArgumentNotNull(leftSequence, "leftSequence");
      ArgumentValidator.EnsureArgumentNotNull(rightSequence, "rightSequence");
      using (var leftEnum = leftSequence.GetEnumerator())
      using (var rightEnum = rightSequence.GetEnumerator()) {
        bool hasLeft = leftEnum.MoveNext();
        bool hasRight = rightEnum.MoveNext();
        while (hasLeft && hasRight) {
          yield return projector(leftEnum.Current, rightEnum.Current);
          hasLeft = leftEnum.MoveNext();
          hasRight = rightEnum.MoveNext();
        }
        while (hasLeft) {
          yield return projector(leftEnum.Current, default(TRight));
          hasLeft = leftEnum.MoveNext();
        }
        while (hasRight) {
          yield return projector(default(TLeft), rightEnum.Current);
          hasRight = rightEnum.MoveNext();
        }
      }
    }

    /// <summary>
    /// Constructs <see cref="IEnumerable{T}"/> from
    /// this <see cref="IEnumerable{T}"/> and specified <see cref="IEnumerable{T}"/>
    /// by creating a <see cref="Pair{TFirst,TSecond}"/> from each pair of items.
    /// If one input <see cref="IEnumerable{T}"/> is short, it is extended with default values.
    /// </summary>
    /// <typeparam name="TLeft">Type of first <see cref="IEnumerable{T}"/>.</typeparam>
    /// <typeparam name="TRight">Type of second <see cref="IEnumerable{T}"/>.</typeparam>
    /// <param name="leftSequence">First <see cref="IEnumerable{T}"/>.</param>
    /// <param name="rightSequence">Second <see cref="IEnumerable{T}"/>.</param>
    /// <returns>Zip result.</returns>
    public static IEnumerable<Pair<TLeft, TRight>> ZipExtend<TLeft, TRight>(
      this IEnumerable<TLeft> leftSequence, IEnumerable<TRight> rightSequence)
    {
      return ZipExtend(leftSequence, rightSequence, (l, r) => new Pair<TLeft, TRight>(l, r));
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

    /// <summary>
    /// Safely adds one value to sequence.
    /// </summary>
    /// <typeparam name="T">The type of enumerated items.</typeparam>
    /// <param name="source">Source sequence.</param>
    /// <param name="value">Value to add to sequence.</param>
    /// <returns>New sequence with both <paramref name="source"/> and <paramref name="value"/> items inside without duplicates.</returns>
    /// <remarks>If source sequence is null, it's equals to empty sequence. If value is null, it will not added to result sequence.</remarks>
    public static IEnumerable<T> AddOne<T>(this IEnumerable<T> source, T value)
    {
      source = source ?? EnumerableUtils<T>.Empty;
      if (!ReferenceEquals(value, null))
        source = source.Union(EnumerableUtils.One(value));
      return source;
    }
  }
}
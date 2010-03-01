// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.05.16

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;
using Xtensive.Core.Resources;

namespace System
{
  /// <summary>
  /// <see cref="IEnumerable{T}"/> related extension methods.
  /// </summary>
  public static class EnumerableExtensions
  {
    private const int defaultInitialBatchSize = 8;
    private const int defaultMaximalBatchSize = 1024;
    private const int defaultFirstFastCount = 0;

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
      var stringValue = items as string;
      if (stringValue!=null)
        return stringValue.Length==0;
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
    /// "Runs" the specified <paramref name="sequence"/> by enumerating it.
    /// </summary>
    /// <typeparam name="T">Type of the sequence item.</typeparam>
    /// <param name="sequence">The sequence to run.</param>
    public static void Run<T>(this IEnumerable<T> sequence)
    {
      foreach (var item in sequence);
    }

    /// <summary>
    /// Applies the specified <paramref name="action"/> to all the items 
    /// from the <paramref name="items"/> sequence.
    /// </summary>
    /// <typeparam name="T">Type of the sequence item.</typeparam>
    /// <param name="items">The sequence to apply the <paramref name="action"/> to.</param>
    /// <param name="action">The action to apply.</param>
    public static void ForEach<T>(this IEnumerable<T> items, Action<T, int> action)
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
    public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
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
    /// Calculates hash code of <see cref="IEnumerable{T}"/> by XOR hash codes of it's items.
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
        sb.Append(item);
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
        sb.Append(item);
        insertDelimiter = true;
      }
      return sb.ToString();
    }

    /// <summary>
    /// Determines whether this <see cref="IEnumerable{T}"/> equals to another, 
    /// i.e. contains the same items in the same order.
    /// </summary>
    /// <typeparam name="TItem">The type of item.</typeparam>
    /// <param name="items">This <see cref="IEnumerable{T}"/>.</param>
    /// <param name="other">The <see cref="IEnumerable{T}"/> to compare with.</param>
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
      ArgumentValidator.EnsureArgumentNotNull(projector, "projector");

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
    /// If <paramref name="sequence"/> is not <see langword="null"/>, creates an array from <see cref="IEnumerable{T}"/>.
    /// Otherwise, returns empty array.
    /// </summary>
    /// <typeparam name="T">Element type</typeparam>
    /// <param name="sequence">The sequence.</param>
    /// <returns>Array of elements of <paramref name="sequence"/>
    /// or empty array, if <paramref name="sequence"/> is <see langword="null"/>.</returns>
    public static T[] ToArraySafely<T>(this IEnumerable<T> sequence)
    {
      if (sequence == null)
        return ArrayUtils<T>.EmptyArray;
      return sequence.ToArray();
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
        source = source.Concat(EnumerableUtils.One(value));
      return source;
    }

    /// <summary>
    /// Splits the specified <see cref="IEnumerable{T}"/> into batches.
    /// </summary>
    /// <typeparam name="T">The type of enumerated items.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="firstFastCount">The count of the source sequence's items 
    /// which will be returned without batching.</param>
    /// <param name="initialBatchSize">The initial size of a batch.</param>
    /// <param name="maximalBatchSize">The maximal sized of a batch.</param>
    /// <returns>The source sequence split into batches.</returns>
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int firstFastCount,
      int initialBatchSize, int maximalBatchSize)
    {
      ArgumentValidator.EnsureArgumentIsInRange(initialBatchSize, 0, int.MaxValue, "initialBatchSize");
      ArgumentValidator.EnsureArgumentIsInRange(maximalBatchSize, 0, int.MaxValue, "maximalBatchSize");
      if(maximalBatchSize < initialBatchSize)
        throw new ArgumentException(String.Format(Strings.ExArgumentXIsLessThanArgumentY,
          "maximalBatchSize", "initialBatchSize"));
      var currentCount = 0;
      var currentBatchSize = initialBatchSize;
      using (var enumerator = source.GetEnumerator()) {
        while (currentCount < firstFastCount && enumerator.MoveNext()) {
          currentCount++;
          yield return EnumerableUtils.One(enumerator.Current);
        }
        while (enumerator.MoveNext()) {
          currentCount = 0;
          var batch = new List<T>(currentBatchSize);
          do {
            batch.Add(enumerator.Current);
            currentCount++;
          } while (currentCount < currentBatchSize && enumerator.MoveNext());
          if(currentBatchSize < maximalBatchSize) {
            currentBatchSize *= 2;
            if(currentBatchSize > maximalBatchSize)
              currentBatchSize = maximalBatchSize;
          }
          yield return batch;
        }
      }
    }

    /// <summary>
    /// Splits the specified <see cref="IEnumerable{T}"/> into batches.
    /// </summary>
    /// <typeparam name="T">The type of enumerated items.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <returns>The source sequence split into batches.</returns>
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source)
    {
      return source.Batch(defaultFirstFastCount, defaultInitialBatchSize, defaultMaximalBatchSize);
    }

    /// <summary>
    /// Splits the specified <see cref="IEnumerable{T}"/> into batches.
    /// </summary>
    /// <typeparam name="T">The type of enumerated items.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="firstFastCount">The count of the source sequence's items 
    /// which will be returned without batching.</param>
    /// <returns>The source sequence split into batches.</returns>
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int firstFastCount)
    {
      return source.Batch(firstFastCount, defaultInitialBatchSize, defaultMaximalBatchSize);
    }

    /// <summary>
    /// Invokes the specified delegate before the enumeration of each batch.
    /// </summary>
    /// <typeparam name="T">The type of enumerated items.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="action">The delegate that will be invoked before 
    /// the enumeration of each batch.</param>
    /// <returns>The source sequence.</returns>
    public static IEnumerable<IEnumerable<T>> ApplyBefore<T>(this IEnumerable<IEnumerable<T>> source,
      Action action)
    {
      ArgumentValidator.EnsureArgumentNotNull(action, "action");
      return source.ApplyBeforeAndAfter(action, null);
    }

    /// <summary>
    /// Invokes the specified delegate after the enumeration of each batch.
    /// </summary>
    /// <typeparam name="T">The type of enumerated items.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="action">The delegate that will be invoked after 
    /// the enumeration of each batch.</param>
    /// <returns>The source sequence.</returns>
    public static IEnumerable<IEnumerable<T>> ApplyAfter<T>(this IEnumerable<IEnumerable<T>> source,
      Action action)
    {
      ArgumentValidator.EnsureArgumentNotNull(action, "action");
      return source.ApplyBeforeAndAfter(null, action);
    }

    /// <summary>
    /// Invokes specified delegates before and after the enumeration of each batch.
    /// </summary>
    /// <typeparam name="T">The type of enumerated items.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="beforeAction">The delegate that will be invoked before 
    /// the enumeration of each batch. Set this parameter to <see langword="null" /> to omit 
    /// the invocation.</param>
    /// <param name="afterAction">The delegate that will be invoked after 
    /// the enumeration of each batch. Set this parameter to <see langword="null" /> to omit 
    /// the invocation.
    /// <returns>The source sequence.</returns>
    public static IEnumerable<IEnumerable<T>> ApplyBeforeAndAfter<T>(this IEnumerable<IEnumerable<T>> source,
      Action beforeAction, Action afterAction)
    {
      using (var enumerator = source.GetEnumerator()) {
        while (true) {
          if(beforeAction != null)
            beforeAction.Invoke();
          IEnumerable<T> batch;
          try {
            if (!enumerator.MoveNext())
              yield break;
            batch = enumerator.Current;
          }
          finally {
            if (afterAction!=null)
              afterAction.Invoke();
          }
          yield return batch;
        }
      }
    }

    /// <summary>
    /// Flattens the item's hierarchy.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="root">The root of the hierarchy.</param>
    /// <param name="childrenExtractor">The children extractor. It's always executed 
    /// before a root item will be returned.</param>
    /// <param name="exitAction">This action is always executed after a root item 
    /// was returned.</param>
    /// <param name="rootFirst">If set to <see langword="true"/> then a root item 
    /// will be returned before its children.</param>
    /// <returns>The <see cref="IEnumerable{T}"/> containing all items in the 
    /// specified hierarchy.</returns>
    public static IEnumerable<TItem> Flatten<TItem>(this IEnumerable<TItem> root,
      Func<TItem, IEnumerable<TItem>> childrenExtractor, Action<TItem> exitAction, bool rootFirst)
    {
      // The validation of arguments is omitted to increase performance.
      foreach (var item in root) {
        var children = childrenExtractor.Invoke(item);
        if(rootFirst)
          yield return item;
        if(children != null)
          foreach (var childItem in children.Flatten(childrenExtractor, exitAction, rootFirst))
            yield return childItem;
        if(!rootFirst)
          yield return item;
        if(exitAction != null)
          exitAction.Invoke(item);
      }
    }

    /// <summary>
    /// Determines whenever specified sequence contains at least <see cref="numberOfElements"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="sequence">The sequence.</param>
    /// <param name="numberOfElements">The number of elements.</param>
    /// <returns><see langword="true"/> if <paramref name="sequence"/> contains at least <paramref name="numberOfElements"/>;
    /// <see langword="false"/> otherwise.</returns>
    public static bool AtLeast<TItem>(this IEnumerable<TItem> sequence, int numberOfElements)
    {
      using (var enumerator = sequence.GetEnumerator()) {
        while (numberOfElements > 0 && enumerator.MoveNext())
          --numberOfElements;
        return numberOfElements <= 0;
      }
    }

    /// <summary>
    /// Determines whenever specified sequence contains at most <see cref="numberOfElements"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="sequence">The sequence.</param>
    /// <param name="numberOfElements">The number of elements.</param>
    /// <returns><see langword="true"/> if <paramref name="sequence"/> contains at most <paramref name="numberOfElements"/>;
    /// <see langword="false"/> otherwise.</returns>
    public static bool AtMost<TItem>(this IEnumerable<TItem> sequence, int numberOfElements)
    {
      using (var enumerator = sequence.GetEnumerator()) {
        while (numberOfElements >= 0 && enumerator.MoveNext())
          --numberOfElements;
        return numberOfElements >= 0;
      }
    }
  }
}
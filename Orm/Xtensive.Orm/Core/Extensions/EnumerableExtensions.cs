// Copyright (C) 2008-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.05.16

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Collections;
using Xtensive.Collections.Graphs;
using Xtensive.Orm;


namespace Xtensive.Core
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
    /// by attempting to cast it to <see cref="ICollection{T}"/> and <see cref="IQueryable{T}"/>.
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
    /// <see cref="ICollection{T}"/> or <see cref="IQueryable{T}"/>.
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
        var c1 = items as IQueryable<TItem>;
        if (c1 != null)
          return c1.LongCount();
      }
      return null;
    }

    /// <summary>
    /// Gets the count of items of <see cref="IEnumerable{T}"/>, if it is actually
    /// <see cref="ICollection{T}"/> or <see cref="IQueryable{T}"/>.
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
    public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
    {
      foreach (var item in items)
        action.Invoke(item);
    }

    /// <summary>
    /// Converts the sequence to the <see cref="ChainedBuffer{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of sequence item.</typeparam>
    /// <param name="source">The sequence to convert.</param>
    /// <returns>A new <see cref="ChainedBuffer{T}"/> instance containing
    /// all the items from the <paramref name="source"/> sequence.</returns>
    public static ChainedBuffer<T> ToChainedBuffer<T>(this IEnumerable<T> source)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      return new ChainedBuffer<T>(source);
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
      var sb = new ValueStringBuilder(stackalloc char[4096]);
      bool insertDelimiter = false;
      foreach (var item in source) {
        if (insertDelimiter)
          sb.Append(delimiter);
        sb.Append(item?.ToString());
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
      var sb = new ValueStringBuilder(stackalloc char[4096]);
      bool insertDelimiter = false;
      foreach (object item in source) {
        if (insertDelimiter)
          sb.Append(separator);
        sb.Append(item?.ToString());
        insertDelimiter = true;
      }
      return sb.ToString();
    }

    /// <summary>
    /// If <paramref name="sequence"/> is not <see langword="null"/>, creates an array from <see cref="IEnumerable{T}"/>.
    /// Otherwise, returns empty array.
    /// </summary>
    /// <typeparam name="T">Element type</typeparam>
    /// <param name="sequence">The sequence.</param>
    /// <returns>Array of elements of <paramref name="sequence"/>
    /// or empty array, if <paramref name="sequence"/> is <see langword="null"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] ToArraySafely<T>(this IEnumerable<T> sequence)
    {
      if (sequence == null)
        return Array.Empty<T>();
      return sequence.ToArray();
    }

    /// <summary>
    /// Same as <code>sequence.ExtendWithDefaultsTo(length).Take(length).ToArray()</code>,
    /// but with a single allocation (of a resulting array).
    /// </summary>
    /// <param name="sequence">Source sequence.</param>
    /// <param name="length">The length of the resulting array.</param>
    /// <typeparam name="T">The sequence element type.</typeparam>
    /// <returns>The resulting array.</returns>
    /// <exception cref="ArgumentException"><paramref name="length"/> is negative.</exception>
    public static T[] ToArray<T>(this IEnumerable<T> sequence, int length)
    {
      ArgumentValidator.EnsureArgumentIsGreaterThanOrEqual(length, 0, nameof(length));

      if (length == 0) {
        return Array.Empty<T>();
      }

      var result = new T[length];
      using (var e = sequence.GetEnumerator()) {
        for (var i = 0; i < length && e.MoveNext(); i++) {
          result[i] = e.Current;
        }
      }
      return result;
    }

    /// <summary>
    /// Creates a <see cref="T:System.Collections.Generic.List`1" /> of specified capacity from an <see cref="T:System.Collections.Generic.IEnumerable`1"/>.
    /// </summary>
    /// <param name="source">The <see cref="T:System.Collections.Generic.IEnumerable`1" /> to create a <see cref="T:System.Collections.Generic.List`1" /> from.</param>
    /// <param name="capacity">The number of elements that the new list can initially store.</param>
    /// <typeparam name="TItem">The type of the elements of <paramref name="source" />.</typeparam>
    /// <returns>A <see cref="T:System.Collections.Generic.List`1" /> that contains elements from the input sequence.</returns>
    public static List<TItem> ToList<TItem>(this IEnumerable<TItem> source, int capacity)
    {
      ArgumentValidator.EnsureArgumentIsGreaterThanOrEqual(capacity, 0, "capacity");

      var result = new List<TItem>(capacity);
      result.AddRange(source);
      return result;
    }

    /// <summary>
    /// Creates a <see cref="Dictionary{TKey, TValue}"/> with pre-defined capacity from an <see cref="IEnumerable{T}"/>
    /// according to a specified key selector function.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <typeparam name="TKey">The type of the key elements.</typeparam>
    /// <param name="source">A sequence to create a <see cref="Dictionary{TKey, TValue}"/> from.</param>
    /// <param name="keySelector">A function to extract a key from each element.</param>
    /// <param name="capacity">Initial dictionary capacity.</param>
    /// <returns>A <see cref="Dictionary{TKey, TValue}"/> that contains keys and values.</returns>
    public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, int capacity)
    {
      return ToDictionary<TSource, TKey, TSource>(source, keySelector, InstanceSelector, capacity);

      static TSource InstanceSelector(TSource x)
      {
        return x;
      }
    }

    /// <summary>
    /// Creates a <see cref="Dictionary{TKey, TValue}"/> with pre-defined capacity from an <see cref="IEnumerable{T}"/>
    /// according to a specified key selector function.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <typeparam name="TKey">The type of the key elements.</typeparam>
    /// <param name="source">A sequence to create a <see cref="Dictionary{TKey, TValue}"/> from.</param>
    /// <param name="keySelector">A function to extract a key from each element.</param>
    /// <param name="comparer">An <see cref="IEqualityComparer{T}"/> to compare keys.</param>
    /// <param name="capacity">Initial dictionary capacity.</param>
    /// <returns>A <see cref="Dictionary{TKey, TValue}"/> that contains keys and values.</returns>
    public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(
      this IEnumerable<TSource> source,
      Func<TSource, TKey> keySelector,
      IEqualityComparer<TKey> comparer,
      int capacity)
    {
      return ToDictionary<TSource, TKey, TSource>(source, keySelector, InstanceSelector, comparer, capacity);

      static TSource InstanceSelector(TSource x)
      {
        return x;
      }
    }

    /// <summary>
    /// Creates a <see cref="Dictionary{TKey, TValue}"/> with pre-defined capacity from an <see cref="IEnumerable{T}"/>
    /// according to a specified key selector function.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <typeparam name="TKey">The type of the key elements.</typeparam>
    /// <typeparam name="TValue">The type of values in result dictionary</typeparam>
    /// <param name="source">A sequence to create a <see cref="Dictionary{TKey, TValue}"/> from.</param>
    /// <param name="keySelector">A function to extract a key from each element.</param>
    /// <param name="elementSelector">A funtion to extract a value from each element.</param>
    /// <param name="capacity">Initial dictionary capacity.</param>
    /// <returns>A <see cref="Dictionary{TKey, TValue}"/> that contains keys and values.</returns>
    public static Dictionary<TKey, TValue> ToDictionary<TSource, TKey, TValue>(
      this IEnumerable<TSource> source,
      Func<TSource, TKey> keySelector,
      Func<TSource, TValue> elementSelector,
      int capacity)
    {
      return ToDictionary(source, keySelector, elementSelector, null, capacity);
    }

    /// <summary>
    /// Creates a <see cref="Dictionary{TKey, TValue}"/> with pre-defined capacity from an <see cref="IEnumerable{T}"/>
    /// according to a specified key selector function.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <typeparam name="TKey">The type of the key elements.</typeparam>
    /// <typeparam name="TValue">The type of values in result dictionary</typeparam>
    /// <param name="source">A sequence to create a <see cref="Dictionary{TKey, TValue}"/> from.</param>
    /// <param name="keySelector">A function to extract a key from each element.</param>
    /// <param name="elementSelector">A funtion to extract a value from each element.</param>
    /// <param name="comparer">An <see cref="IEqualityComparer{T}"/> to compare keys.</param>
    /// <param name="capacity">Initial dictionary capacity.</param>
    /// <returns>A <see cref="Dictionary{TKey, TValue}"/> that contains keys and values.</returns>
    public static Dictionary<TKey, TValue> ToDictionary<TSource, TKey, TValue>(
      this IEnumerable<TSource> source,
      Func<TSource, TKey> keySelector,
      Func<TSource, TValue> elementSelector,
      IEqualityComparer<TKey> comparer,
      int capacity)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, nameof(source));
      ArgumentValidator.EnsureArgumentNotNull(keySelector, nameof(keySelector));
      ArgumentValidator.EnsureArgumentNotNull(elementSelector, nameof(elementSelector));
      ArgumentValidator.EnsureArgumentIsGreaterThanOrEqual(capacity, 0, nameof(capacity));

      var dictionary = comparer != null
        ? new Dictionary<TKey, TValue>(capacity, comparer)
        : new Dictionary<TKey, TValue>(capacity);
      foreach (var item in source) {
        dictionary.Add(keySelector(item), elementSelector(item));
      }
      return dictionary;
    }

    /// <summary>
    /// Gets the items from the segment.
    /// </summary>
    /// <param name="segment">The segment.</param>
    public static IEnumerable<int> GetItems(this in Segment<int> segment)
    {
      return Enumerable.Range(segment.Offset, segment.Length);
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
          if (currentBatchSize < maximalBatchSize) {
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
    /// Invokes specified delegates before and after the enumeration of each batch.
    /// </summary>
    /// <typeparam name="T">The type of enumerated items.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="beforeAction">The delegate that will be invoked before
    /// the enumeration of each batch. Set this parameter to <see langword="null" /> to omit
    /// the invocation.</param>
    /// <param name="afterAction">The delegate that will be invoked after
    /// the enumeration of each batch. Set this parameter to <see langword="null" /> to omit
    /// the invocation.</param>
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
    /// Determines whenever specified sequence contains at least <paramref name="numberOfElements"/>.
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
    /// Determines whenever specified sequence contains at most <paramref name="numberOfElements"/>.
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

    /// <summary>
    /// Sorts <paramref name="values"/> in topological order according to <paramref name="edgeTester"/>.
    /// </summary>
    /// <typeparam name="TValue">Type of a value to sort.</typeparam>
    /// <param name="values">Values to sort.</param>
    /// <param name="edgeTester">A predicate for testing edge presence.</param>
    /// <returns>Topologically sorted <paramref name="values"/> if no cycles exist, otherwise null.</returns>
    public static List<TValue> SortTopologically<TValue>(this IEnumerable<TValue> values, Func<TValue, TValue, bool> edgeTester)
    {
      ArgumentValidator.EnsureArgumentNotNull(values, "values");
      ArgumentValidator.EnsureArgumentNotNull(edgeTester, "edgeTester");

      var graph = new Graph<Node<TValue>, Edge>();
      graph.Nodes.AddRange(values.Select(p => new Node<TValue>(p)));
      foreach (var left in graph.Nodes)
        foreach (var right in graph.Nodes)
          if (edgeTester.Invoke(left.Value, right.Value))
            new Edge(left, right);
      var result = TopologicalSorter.Sort(graph);
      return result.HasLoops ? null : result.SortedNodes.SelectToList(node => node.Value);
    }
  }
}

// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.12

using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Typed ordered enumerable of <typeparamref name="TItem"/> instances.
  /// </summary>
  /// <remarks>
  /// Items are ordered by <typeparamref name="TKey"/>.
  /// </remarks>
  /// <typeparam name="TKey">Type of item key.</typeparam>
  /// <typeparam name="TItem">Item type.</typeparam>
  public interface IOrderedEnumerable<TKey, TItem> : IEnumerable<TItem>,
    IHasKeyExtractor<TKey, TItem>,
    IHasKeyComparers<TKey>
  {
    /// <summary>
    /// Gets the <see cref="IEnumerable{T}"/> enumerating all the <typeparamref name="TKey"/>s in the given range.
    /// </summary>
    /// <param name="range">A <see cref="Range{T}"/> to enumerate through.</param>
    /// <returns>The <see cref="IEnumerable{T}"/> for the range.</returns>
    IEnumerable<TKey> GetKeys(Range<Entire<TKey>> range);

    /// <summary>
    /// Gets the <see cref="IEnumerable{T}"/> enumerating all the <typeparamref name="TItem"/>s in the given range.
    /// </summary>
    /// <param name="range">A <see cref="Range{T}"/> to enumerate through.</param>
    /// <returns>The <see cref="IEnumerable{T}"/> for the range.</returns>
    IEnumerable<TItem> GetItems(Range<Entire<TKey>> range);

    /// <summary>
    /// Gets the <see cref="IEnumerable{T}"/> enumerating all the <typeparamref name="TItem"/>s in the given range set.
    /// </summary>
    /// <param name="range">A <see cref="Range{T}"/> to enumerate through.</param>
    /// <returns>The <see cref="IEnumerable{T}"/> for the range set.</returns>
    IEnumerable<TItem> GetItems(RangeSet<Entire<TKey>> range);

    /// <summary>
    /// Seeks for the specified item in the ordered enumerable.
    /// </summary>
    /// <param name="ray">Item to locate.</param>
    /// <returns>Result of seek operation.</returns>
    SeekResult<TItem> Seek(Ray<Entire<TKey>> ray);

    /// <summary>
    /// Seeks for the specified item in the ordered enumerable.
    /// </summary>
    /// <param name="key">Item key to locate.</param>
    /// <returns>Result of seek operation.</returns>
    SeekResult<TItem> Seek(TKey key);

    /// <summary>
    /// Creates <see cref="IIndexReader{TKey,TItem}"/> object allowing to enumerate 
    /// the items in the specified <paramref name="range"/>.
    /// </summary>
    /// <param name="range">The range to provide the reader for.</param>
    IIndexReader<TKey, TItem> CreateReader(Range<Entire<TKey>> range);
  }
}
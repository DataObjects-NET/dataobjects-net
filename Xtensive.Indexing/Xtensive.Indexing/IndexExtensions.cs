// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.06.19

using Xtensive.Core;

namespace Xtensive.Indexing
{
  /// <summary>
  /// <see cref="IIndex{TKey,TItem}"/> and its "inner" (supported) interfaces extension methods.
  /// </summary>
  public static class IndexExtensions
  {
    /// <summary>
    /// Gets the <see cref="Range{T}"/> object describing full key range 
    /// for the specified <paramref name="index"/>
    /// which endpoints are ordered in <see cref="Direction.Positive"/> direction.
    /// </summary>
    /// <typeparam name="TKey">The type of the <paramref name="index"/> key.</typeparam>
    /// <param name="index">The index to get the full range for.</param>
    /// <returns><see cref="Range{T}"/> object describing full key range 
    /// which endpoints are ordered in <see cref="Direction.Positive"/> direction.</returns>
    public static Range<Entire<TKey>> GetFullRange<TKey>(this IHasKeyComparers<TKey> index)
    {
      return GetFullRange(index, Direction.Positive);
    }

    /// <summary>
    /// Gets the <see cref="Range{T}"/> object describing full key range 
    /// for the specified <paramref name="index"/> 
    /// which endpoints are ordered in specified <paramref name="direction"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the <paramref name="index"/> key.</typeparam>
    /// <param name="index">The index to get the full range for.</param>
    /// <param name="direction">The direction of the range endpoints.</param>
    /// <returns><see cref="Range{T}"/> object describing full key range 
    /// which endpoints are ordered in specified <paramref name="direction"/>.</returns>
    public static Range<Entire<TKey>> GetFullRange<TKey>(this IHasKeyComparers<TKey> index, Direction direction)
    {
      return Range<Entire<TKey>>.Full.Redirect(direction, index.EntireKeyComparer);
    }
  }
}
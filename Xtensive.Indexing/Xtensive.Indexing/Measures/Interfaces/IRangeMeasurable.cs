// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.11.14

using System;

namespace Xtensive.Indexing.Measures
{
  /// <summary>
  /// Range measurable - an <see cref="IMeasurable{TItem}"/> providing a set of 
  /// associated <see cref="IMeasure{T,TResult}"/> objects for any of its <see cref="Range{T}"/>s.
  /// </summary>
  /// <typeparam name="TKey">The range key.</typeparam>
  /// <typeparam name="TItem">The type of item of underlying <see cref="IMeasurable{TItem}"/>.</typeparam>
  public interface IRangeMeasurable<TKey, TItem>: IMeasurable<TItem>
  {
    /// <summary>
    /// Gets the measurement result for <see cref="IMeasure{TItem,TResult}"/> with the specified <paramref name="name"/>.
    /// </summary>
    /// <param name="range">The range of <typeparamref name="TKey"/>s.</param>
    /// <param name="name">The name of the measure.</param>
    /// <returns>The measurement result.</returns>
    object GetMeasureResult(Range<Entire<TKey>> range, string name);

    /// <summary>
    /// Gets the measurement result for a set of <see cref="IMeasure{TItem,TResult}"/> with the specified <paramref name="names"/>.
    /// </summary>
    /// <param name="range">The range of <typeparamref name="TKey"/>s.</param>
    /// <param name="names">The names of measures to get.</param>
    /// <returns>The <see cref="Array"/> of measurement results for specified <see cref="IMeasure{TItem,TResult}"/>s.</returns>
    object[] GetMeasureResults(Range<Entire<TKey>> range, params string[] names);
  }
}
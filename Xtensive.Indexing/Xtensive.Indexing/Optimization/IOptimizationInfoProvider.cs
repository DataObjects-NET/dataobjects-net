// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.13

using Xtensive.Comparison;

namespace Xtensive.Indexing.Optimization
{
  /// <summary>
  /// Statistics provider.
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  public interface IOptimizationInfoProvider<TKey>
  {
    /// <summary>
    /// Gets the <see cref="IStatistics{T}"/>.
    /// </summary>
    /// <returns>The statistics.</returns>
    IStatistics<TKey> GetStatistics();

    /// <summary>
    /// Gets the <see cref="AdvancedComparer{T}"/> which is used by the index to compare keys.
    /// </summary>
    /// <returns>The comparer.</returns>
    AdvancedComparer<Entire<TKey>> GetEntireKeyComparer();
  }
}
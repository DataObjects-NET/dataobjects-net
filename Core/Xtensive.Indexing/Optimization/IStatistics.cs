// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.13

namespace Xtensive.Indexing.Optimization
{
  /// <summary>
  /// Statistics of an index.
  /// </summary>
  /// <typeparam name="T">The type of the key of the index.</typeparam>
  public interface IStatistics<T>
  {
    /// <summary>
    /// Gets the <see cref="StatisticsData"/>.
    /// </summary>
    /// <param name="range">The range.</param>
    /// <returns>The statistics data which was collected for the <paramref name="range"/>.</returns>
    StatisticsData GetData(Range<Entire<T>> range);
  }
}
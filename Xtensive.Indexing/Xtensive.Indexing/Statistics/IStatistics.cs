// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.13

namespace Xtensive.Indexing.Statistics
{
  /// <summary>
  /// Statistics of an index.
  /// </summary>
  public interface IStatistics<T>
  {
    /// <summary>
    /// </summary>
    /// <param name="range">The range.</param>
    /// <returns>The statistics data which was collected for the <paramref name="range"/>.</returns>
    StatisticsData GetData(Range<Entire<T>> range);
  }
}
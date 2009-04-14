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
    /// <returns>The count of records which are contained in <paramref name="range"/></returns>
    double GetRecordCount(Range<Entire<T>> range);

    /// <summary>
    /// </summary>
    /// <param name="range">The range.</param>
    /// <returns>The total size of records which are contained in <paramref name="range"/></returns>
    double GetSize(Range<Entire<T>> range);

    /// <summary>
    /// </summary>
    /// <param name="range">The range.</param>
    /// <returns>The count of seeks which are required to load records contained in
    /// <paramref name="range"/></returns>
    double GetSeekCount(Range<Entire<T>> range);
  }
}
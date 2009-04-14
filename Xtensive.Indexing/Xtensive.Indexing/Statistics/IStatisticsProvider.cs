// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.13

namespace Xtensive.Indexing.Statistics
{
  /// <summary>
  /// Statistics provider.
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  public interface IStatisticsProvider<TKey>
  {
    /// <summary>
    /// </summary>
    /// <returns>The statistics.</returns>
    IStatistics<TKey> GetStatistics();
  }
}
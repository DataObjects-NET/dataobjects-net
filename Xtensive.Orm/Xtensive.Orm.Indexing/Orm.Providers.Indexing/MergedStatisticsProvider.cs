// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.14

using System;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Indexing;
using Xtensive.Indexing.Optimization;

namespace Xtensive.Orm.Providers.Indexing
{
  [Serializable]
  internal sealed class MergedStatisticsProvider : IOptimizationInfoProvider<Tuple>, IStatistics<Tuple>
  {
    private readonly IOptimizationInfoProvider<Tuple>[] underlyingProviders;

    public IStatistics<Tuple> GetStatistics()
    {
      return this;
    }

    public AdvancedComparer<Entire<Tuple>> GetEntireKeyComparer()
    {
      return underlyingProviders[0].GetEntireKeyComparer();
    }

    #region Implementation of IStatistics<Tuple>

    public StatisticsData GetData(Range<Entire<Tuple>> range)
    {
      double summaryRecordCount = 0;
      double summarySeekCount = 0;
      foreach (var provider in underlyingProviders) {
        var currentData = provider.GetStatistics().GetData(range);
        summaryRecordCount += currentData.RecordCount;
        summarySeekCount += currentData.SeekCount;
      }
      return new StatisticsData(summaryRecordCount, summarySeekCount);
    }

    #endregion


    // Constructors

    /// <exception cref="Exception"><paramref name="underlyingProviders"/> is empty.</exception>
    public MergedStatisticsProvider(IOptimizationInfoProvider<Tuple>[] underlyingProviders)
    {
      ArgumentValidator.EnsureArgumentNotNull(underlyingProviders, "underlyingProviders");
      if (underlyingProviders.Length == 0)
        throw Exceptions.CollectionIsEmpty("underlyingProviders");
      this.underlyingProviders = underlyingProviders;
    }
  }
}
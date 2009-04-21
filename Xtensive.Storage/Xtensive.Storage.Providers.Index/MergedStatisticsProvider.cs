// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.14

using System;
using Xtensive.Core;
using Xtensive.Core.Comparison;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Indexing.Optimization;

namespace Xtensive.Storage.Providers.Index
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

    public MergedStatisticsProvider(IOptimizationInfoProvider<Tuple>[] underlyingProviders)
    {
      ArgumentValidator.EnsureArgumentNotNull(underlyingProviders, "underlyingProviders");
      if (underlyingProviders.Length == 0)
        throw new ArgumentException(Resources.Strings.ExCollectionMustNotBeEmpty, "underlyingProviders");
      this.underlyingProviders = underlyingProviders;
    }
  }
}
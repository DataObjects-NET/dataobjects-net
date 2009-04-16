// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.14

using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Indexing.Statistics;

namespace Xtensive.Storage.Providers.Index
{
  [Serializable]
  internal sealed class MergedStatisticsProvider : IStatisticsProvider<Tuple>, IStatistics<Tuple>
  {
    private readonly IStatisticsProvider<Tuple>[] underlyingProviders;

    public IStatistics<Tuple> GetStatistics()
    {
      return this;
    }

    #region Implementation of IStatistics<Tuple>

    public double GetRecordCount(Range<Entire<Tuple>> range)
    {
      return underlyingProviders.Sum(provider => provider.GetStatistics().GetRecordCount(range));
    }

    public double GetSize(Range<Entire<Tuple>> range)
    {
      return underlyingProviders.Sum(provider => provider.GetStatistics().GetSize(range));
    }

    public double GetSeekCount(Range<Entire<Tuple>> range)
    {
      return underlyingProviders.Sum(provider => provider.GetStatistics().GetSeekCount(range));
    }

    #endregion


    // Constructors

    public MergedStatisticsProvider(IStatisticsProvider<Tuple>[] underlyingProviders)
    {
      ArgumentValidator.EnsureArgumentNotNull(underlyingProviders, "underlyingProviders");
      if (underlyingProviders.Length == 0)
        throw new ArgumentException(Resources.Strings.ExCollectionMustNotBeEmpty, "underlyingProviders");
      this.underlyingProviders = underlyingProviders;
    }
  }
}
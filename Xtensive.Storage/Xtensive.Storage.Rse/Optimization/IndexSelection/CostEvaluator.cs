// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.14

using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse.Optimization.IndexSelection
{
  internal class CostEvaluator : ICostEvaluator
  {
    private readonly StatisticsProviderResolver providerResolver;

    #region Implementation of ICostEvaluator

    public double Evaluate(IndexInfo indexInfo, RangeSet<Entire<Tuple>> rangeSet)
    {
      ArgumentValidator.EnsureArgumentNotNull(indexInfo, "indexInfo");
      ArgumentValidator.EnsureArgumentNotNull(rangeSet, "rangeSet");
      double result = 0;
      var statistics = providerResolver.Resolve(indexInfo).GetStatistics();
      foreach (var range in rangeSet)
        result += statistics.GetRecordCount(range) + statistics.GetSize(range)
          + statistics.GetSeekCount(range);
      return result;
    }

    #endregion


    // Constructors

    public CostEvaluator(StatisticsProviderResolver providerResolver)
    {
      ArgumentValidator.EnsureArgumentNotNull(providerResolver, "providerResolver");
      this.providerResolver = providerResolver;
    }
  }
}
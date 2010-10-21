// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.14

using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Indexing;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse.PreCompilation.Optimization.IndexSelection
{
  internal sealed class CostEvaluator : ICostEvaluator
  {
    private readonly IOptimizationInfoProviderResolver providerResolver;

    #region Implementation of ICostEvaluator

    public CostInfo Evaluate(IndexInfo indexInfo, RangeSet<Entire<Tuple>> rangeSet)
    {
      ArgumentValidator.EnsureArgumentNotNull(indexInfo, "indexInfo");
      ArgumentValidator.EnsureArgumentNotNull(rangeSet, "rangeSet");
      double recordCount = 0;
      double seekCount = 0;
      var statistics = providerResolver.Resolve(indexInfo).GetStatistics();
      foreach (var range in rangeSet) {
        var current = statistics.GetData(range);
        recordCount += current.RecordCount;
        seekCount += current.SeekCount;
      }
      return new CostInfo(recordCount, seekCount);
    }

    #endregion


    // Constructors

    public CostEvaluator(IOptimizationInfoProviderResolver providerResolver)
    {
      ArgumentValidator.EnsureArgumentNotNull(providerResolver, "providerResolver");
      this.providerResolver = providerResolver;
    }
  }
}
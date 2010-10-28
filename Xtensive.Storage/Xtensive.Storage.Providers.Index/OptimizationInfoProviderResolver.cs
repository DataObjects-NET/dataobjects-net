// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.14

using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;
using Xtensive.Indexing.Optimization;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.PreCompilation.Optimization.IndexSelection;

namespace Xtensive.Storage.Providers.Index
{
  [Serializable]
  internal sealed class OptimizationInfoProviderResolver : IOptimizationInfoProviderResolver
  {
    private readonly DomainHandler domainHandler;

    public IOptimizationInfoProvider<Tuple> Resolve(IndexInfo indexInfo)
    {
      if (!indexInfo.IsVirtual) {
        return domainHandler.Storage.GetRealIndexStatisticsAdapter(
          domainHandler.GetStorageIndexInfo(indexInfo));
      }
      if ((indexInfo.Attributes & IndexAttributes.Union) == IndexAttributes.Union) {
        var underlyingProviders = indexInfo.UnderlyingIndexes.Select(index => Resolve(index)).ToArray();
        return new MergedStatisticsProvider(underlyingProviders);
      }
      return Resolve(indexInfo.UnderlyingIndexes.First());
    }


    // Constructors

    public OptimizationInfoProviderResolver(DomainHandler domainHandler)
    {
      ArgumentValidator.EnsureArgumentNotNull(domainHandler, "domainHandler");
      this.domainHandler = domainHandler;
    }
  }
}
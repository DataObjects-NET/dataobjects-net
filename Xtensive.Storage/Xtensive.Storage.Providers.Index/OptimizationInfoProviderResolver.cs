// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.14

using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Indexing.Optimization;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.Optimization;

namespace Xtensive.Storage.Providers.Index
{
  [Serializable]
  internal sealed class OptimizationInfoProviderResolver : IOptimizationInfoProviderResolver
  {
    private readonly DomainHandler domainHandler;

    public IOptimizationInfoProvider<Tuple> Resolve(IndexInfo indexInfo)
    {
      if (!indexInfo.IsVirtual)
        return domainHandler.GetRealIndex(indexInfo);
      if ((indexInfo.Attributes & IndexAttributes.Union) != 0) {
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
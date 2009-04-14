// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.14

using System;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.Optimization.IndexSelection
{
  /// <summary>
  /// Optimizer which uses ranges of index keys.
  /// </summary>
  public class IndexOptimizer : IOptimizer
  {
    private RangeSetExtractor rsExtractor;
    private IIndexSelector indexSelector;
    private ProviderTreeRewriter treeRewriter;

    /// <inheritdoc/>
    public CompilableProvider Optimize(CompilableProvider rootProvider)
    {
      throw new NotImplementedException();
    }


    // Constructors

    /// <summary>
    /// 	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="domainModel">The domain model.</param>
    /// <param name="providerResolver">The statistics provider resolver.</param>
    public IndexOptimizer(DomainModel domainModel, StatisticsProviderResolver providerResolver)
    {
      ArgumentValidator.EnsureArgumentNotNull(domainModel, "domainModel");
      ArgumentValidator.EnsureArgumentNotNull(providerResolver, "providerResolver");
      rsExtractor = new RangeSetExtractor(domainModel);
      indexSelector = new SimpleIndexSelector(new CostEvaluator(providerResolver));
    }
  }
}
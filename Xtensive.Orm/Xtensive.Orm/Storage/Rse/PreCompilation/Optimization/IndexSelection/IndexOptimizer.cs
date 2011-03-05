// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.14

using System;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Orm.Model;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.PreCompilation.Optimization.IndexSelection
{
  /// <summary>
  /// Optimizer that selects indexes and their key's ranges to be used.
  /// </summary>
  [Serializable]
  public sealed class IndexOptimizer : CompilableProviderVisitor, IPreCompiler
  {
    private readonly DomainModel domainModel;
    private readonly IOptimizationInfoProviderResolver providerResolver;

    /// <inheritdoc/>
    public CompilableProvider Process(CompilableProvider rootProvider)
    {
      return new IndexOptimizerVisitor(domainModel, providerResolver).Optimize(rootProvider);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="domainModel">The domain model.</param>
    /// <param name="providerResolver">The statistics provider resolver.</param>
    public IndexOptimizer(DomainModel domainModel, IOptimizationInfoProviderResolver providerResolver)
    {
      ArgumentValidator.EnsureArgumentNotNull(domainModel, "domainModel");
      ArgumentValidator.EnsureArgumentNotNull(providerResolver, "providerResolver");
      this.domainModel = domainModel;
      this.providerResolver = providerResolver;
    }
  }
}
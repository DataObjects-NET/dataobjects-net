// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.03.30

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.Optimization
{
  /// <summary>
  /// Composite optimizer.
  /// </summary>
  [Serializable]
  public sealed class CompositeOptimizer : IOptimizer
  {
    private readonly IOptimizer[] optimizers;

    /// <inheritdoc/>
    public CompilableProvider Optimize(CompilableProvider rootProvider)
    {
      var provider = rootProvider;
      foreach (var optimizer in optimizers)
        provider = optimizer.Optimize(provider);
      return provider;
    }


    // Constructors

    /// <summary>
    /// 	<see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="optimizers">Optimizers to be composed.</param>
    public CompositeOptimizer(params IOptimizer[] optimizers)
    {
      this.optimizers = optimizers;
    }
  }
}
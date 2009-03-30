// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.03.30

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.Compilation.Optimizers
{
  [Serializable]
  public sealed class OptimizationQueue : IOptimizer
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
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public OptimizationQueue(params IOptimizer[] optimizers)
    {
      this.optimizers = optimizers;
    }
  }
}
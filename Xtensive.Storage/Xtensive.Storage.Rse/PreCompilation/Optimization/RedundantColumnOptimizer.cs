// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.17

using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.PreCompilation.Optimization
{
  /// <summary>
  /// <see cref="IOptimizer"/> implementation that removes redundant columns from each provider in tree.
  /// </summary>
  public class RedundantColumnOptimizer : IOptimizer
  {
    /// <inheritdoc/>
    CompilableProvider IOptimizer.Optimize(CompilableProvider rootProvider)
    {
      var selectProvider = rootProvider as SelectProvider;
      if (selectProvider != null)
        return new RedundantColumnRemover(selectProvider).RemoveRedundantColumns();
      return rootProvider;
    }
  }
}
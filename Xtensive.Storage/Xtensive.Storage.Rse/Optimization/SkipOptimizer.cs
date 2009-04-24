// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.04.24

using System;
using System.Diagnostics;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Optimization.Implementation;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.Optimization
{
  /// <summary>
  /// Order by <see cref="IOptimizer"/> implementation.
  /// </summary>
  [Serializable]
  public sealed class SkipOptimizer : IOptimizer
  {
    /// <inheritdoc/>
    CompilableProvider IOptimizer.Optimize(CompilableProvider rootProvider)
    {
      var rewriter = new SkipRewriter(rootProvider);
      return rewriter.Rewrite();
    }
  }
}
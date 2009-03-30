// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.03.30

using System;
using System.Diagnostics;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Compilation.Optimizers.Implementation;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.Compilation.Optimizers
{
  /// <summary>
  /// Order by <see cref="IOptimizer"/> implementation.
  /// </summary>
  [Serializable]
  public sealed class OrderbyOptimizer : IOptimizer
  {
    /// <inheritdoc/>
    CompilableProvider IOptimizer.Optimize(CompilableProvider rootProvider)
    {
      var rewriter = new OrderbyRewriter(rootProvider);
      return rewriter.Rewrite();
    }
  }
}
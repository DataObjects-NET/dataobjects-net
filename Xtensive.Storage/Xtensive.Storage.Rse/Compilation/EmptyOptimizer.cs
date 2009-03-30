// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.03.30

using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.Compilation
{
  /// <summary>
  /// Empty <see cref="IOptimizer"/> implementation.
  /// </summary>
  public sealed class EmptyOptimizer : IOptimizer
  {
    /// <inheritdoc/>
    public CompilableProvider Optimize(CompilableProvider rootProvider)
    {
      return rootProvider;
    }
  }
}
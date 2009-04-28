// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.03.27

using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.Compilation
{
  /// <summary>
  /// Provider's tree optimizer contract.
  /// </summary>
  public interface IOptimizer
  {
    /// <summary>
    /// Optimizes the specified provider's tree.
    /// </summary>
    /// <param name="rootProvider">The root provider.</param>
    /// <returns>The result of optimization.</returns>
    CompilableProvider Optimize(CompilableProvider rootProvider);
  }
}
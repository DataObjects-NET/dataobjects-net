// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.10.12

using System;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.PreCompilation.Optimization
{
  [Serializable]
  /// <summary>
  /// <see cref="IPreCompiler"/> implementation that removes redundant columns from store provider in tree.
  /// </summary>
  public class StoreRedundantColumnOptimizer: IPreCompiler
  {
    /// <inheritdoc/>
    CompilableProvider IPreCompiler.Process(CompilableProvider rootProvider)
    {
      var selectProvider = rootProvider as SelectProvider;
      if (selectProvider != null)
        return new StoreRedundantColumnRemover(selectProvider).RemoveRedundantColumns();
      return rootProvider;
    }
  }
}
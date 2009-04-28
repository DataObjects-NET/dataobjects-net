// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.04.24

using System;
using System.Diagnostics;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.PreCompilation.Correction
{
  /// <summary>
  /// Order by <see cref="IPreCompiler"/> implementation.
  /// </summary>
  [Serializable]
  public sealed class SkipCorrector : IPreCompiler
  {
    /// <inheritdoc/>
    CompilableProvider IPreCompiler.Process(CompilableProvider rootProvider)
    {
      var rewriter = new SkipRewriter(rootProvider);
      return rewriter.Rewrite();
    }
  }
}
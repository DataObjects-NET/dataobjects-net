// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.04.24

using System;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.PreCompilation.Correction
{
  /// <summary>
  /// Inserts a <see cref="RowNumberProvider"/> as 
  /// <see cref="UnaryProvider.Source"/> of a <see cref="SkipProvider"/>
  /// </summary>
  [Serializable]
  public sealed class SkipCorrector : IPreCompiler
  {
    /// <inheritdoc/>
    CompilableProvider IPreCompiler.Process(CompilableProvider rootProvider)
    {
      return new SkipRewriter(rootProvider).Rewrite();
    }
  }
}
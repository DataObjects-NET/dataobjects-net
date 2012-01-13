// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.04.24

using System;
using Xtensive.Orm.Rse.Compilation;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Orm.Rse.Providers.Compilable;

namespace Xtensive.Orm.Rse.PreCompilation.Correction
{
  /// <summary>
  /// Corrects an using of <see cref="SkipProvider"/> and <see cref="TakeProvider"/>
  /// </summary>
  [Serializable]
  public sealed class SkipTakeCorrector : IPreCompiler
  {
    private readonly bool takeSupported;
    private readonly bool skipSupported;

    /// <inheritdoc/>
    CompilableProvider IPreCompiler.Process(CompilableProvider rootProvider)
    {
      return new SkipTakeRewriter(rootProvider, takeSupported, skipSupported).Rewrite();
    }


    // Constructors

    public SkipTakeCorrector(bool takeSupported, bool skipSupported)
    {
      this.takeSupported = takeSupported;
      this.skipSupported = skipSupported;
    }
  }
}
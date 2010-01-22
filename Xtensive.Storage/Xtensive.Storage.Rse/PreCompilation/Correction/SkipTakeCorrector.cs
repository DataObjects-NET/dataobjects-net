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
  /// Corrects an using of <see cref="SkipProvider"/> and <see cref="TakeProvider"/>
  /// </summary>
  [Serializable]
  public sealed class SkipTakeCorrector : IPreCompiler
  {
    private readonly bool limitSupported;
    private readonly bool offsetSupported;

    /// <inheritdoc/>
    CompilableProvider IPreCompiler.Process(CompilableProvider rootProvider)
    {
      return new SkipTakeRewriter(rootProvider, limitSupported, offsetSupported).Rewrite();
    }


    // Constructors

    public SkipTakeCorrector(bool limitSupported, bool offsetSupported)
    {
      this.limitSupported = limitSupported;
      this.offsetSupported = offsetSupported;
    }
  }
}
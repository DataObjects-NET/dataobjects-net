// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.05.15

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.PreCompilation.Correction.ApplyProviderCorrection
{
  /// <summary>
  /// Converts <see cref="ApplyProvider"/> to <see cref="PredicateJoinProvider"/>, if possible.
  /// </summary>
  public sealed class ApplyProviderCorrector : IPreCompiler
  {
    private readonly bool throwOnCorrectionFault;

    /// <inheritdoc/>
    public CompilableProvider Process(CompilableProvider rootProvider)
    {
      return new ApplyProviderCorrectorRewriter(throwOnCorrectionFault).Rewrite(rootProvider);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="throwOnCorrectionFault">if set to <see langword="true"/> 
    /// then <see cref="InvalidOperationException"/> will be thrown in case of 
    /// the correction's fault; otherwise the origin <see cref="CompilableProvider"/> 
    /// will be returned.</param>
    public ApplyProviderCorrector(bool throwOnCorrectionFault)
    {
      this.throwOnCorrectionFault = throwOnCorrectionFault;
    }
  }
}
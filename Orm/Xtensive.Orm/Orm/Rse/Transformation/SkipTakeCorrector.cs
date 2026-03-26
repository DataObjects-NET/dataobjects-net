// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.04.24

using System;
using Xtensive.Orm.Rse.Compilation;
using Xtensive.Orm.Rse.Providers;

namespace Xtensive.Orm.Rse.Transformation
{
  /// <summary>
  /// Corrects an using of <see cref="SkipProvider"/> and <see cref="TakeProvider"/>
  /// </summary>
  [Serializable]
  public sealed class SkipTakeCorrector : IPreCompiler
  {
    private static readonly Lazy<SkipTakeCorrector> fullPagingSupport = new(() => new SkipTakeCorrector(true, true));
    private static readonly Lazy<SkipTakeCorrector> takeOnlySupport = new(() => new SkipTakeCorrector(true, false));
    private static readonly Lazy<SkipTakeCorrector> skipOnlySupport = new(() => new SkipTakeCorrector(false, true));
    private static readonly Lazy<SkipTakeCorrector> noPagingSupport = new(() => new SkipTakeCorrector(false, false));

    private readonly bool takeSupported;
    private readonly bool skipSupported;


    /// <summary>
    /// Gets cached instance of create and cache new one
    /// </summary>
    /// <param name="takeSupported">Take operation is supported.</param>
    /// <param name="skipSupported">Skip operation is supported.</param>
    /// <returns>Cached instance of corrector.</returns>
    public static SkipTakeCorrector GetOrCreate(bool takeSupported, bool skipSupported)
    {
      return (takeSupported, skipSupported) switch {
        (true, true) => fullPagingSupport.Value,
        (false, true) => skipOnlySupport.Value,
        (true, false) => takeOnlySupport.Value,
        (false, false) => noPagingSupport.Value
      };
    }

    /// <inheritdoc/>
    CompilableProvider IPreCompiler.Process(CompilableProvider rootProvider)
    {
      return new SkipTakeRewriter(rootProvider, takeSupported, skipSupported).Rewrite();
    }


    // Constructors

    private SkipTakeCorrector(bool takeSupported, bool skipSupported)
    {
      this.takeSupported = takeSupported;
      this.skipSupported = skipSupported;
    }
  }
}
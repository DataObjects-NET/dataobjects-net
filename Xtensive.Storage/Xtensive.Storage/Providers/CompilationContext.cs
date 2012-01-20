// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.30

using System;
using Xtensive.Storage.Rse.Compilation;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// An implementation of <see cref="Rse.Compilation.CompilationContext"/> suitable for storage.
  /// </summary>
  public sealed class CompilationContext : Rse.Compilation.CompilationContext
  {
    /// <inheritdoc/>
    public override Rse.Providers.EnumerationContext CreateEnumerationContext()
    {
      return Session.Demand().CreateEnumerationContext();
    }


    // Constructors

    /// <inheritdoc/>
    public CompilationContext(
      Func<ICompiler> compilerProvider, 
      Func<IPreCompiler> optimizerProvider,
      Func<ICompiler,IPostCompiler> postCompilerProvider,
      int cacheSize)
      : base(compilerProvider, optimizerProvider, postCompilerProvider, cacheSize)
    {}
  }
}
// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.04

using System;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.Compilation
{
  /// <summary>
  /// Strongly typed version of <see cref="TypeCompiler"/>.
  /// </summary>
  /// <typeparam name="TProvider">The type of the provider this compiler can compile.</typeparam>
  public abstract class TypeCompiler<TProvider> : TypeCompiler
    where TProvider : CompilableProvider
  {
    /// <inheritdoc/>
    public sealed override ExecutableProvider Compile(CompilableProvider provider, params ExecutableProvider[] compiledSources)
    {
      return Compile((TProvider) provider, compiledSources);
    }

    /// <summary>
    /// Compiles the specified provider.
    /// </summary>
    /// <param name="provider">The provider to compile.</param>
    /// <param name="compiledSources">Compiled sources of the <paramref name="provider"/>.</param>
    /// <returns>Compiled provider.</returns>
    protected abstract ExecutableProvider Compile(TProvider provider, params ExecutableProvider[] compiledSources);


    // Constructor

    /// <inheritdoc/>
    protected TypeCompiler(Compiler compiler)
      : base(compiler)
    {
    }
  }
}
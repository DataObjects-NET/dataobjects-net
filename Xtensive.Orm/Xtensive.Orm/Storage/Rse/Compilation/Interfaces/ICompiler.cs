// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.08.31

using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Compilation
{
  /// <summary>
  /// Provider compiler contract.
  /// </summary>
  public interface ICompiler
  {
    /// <summary>
    /// Gets the outer referenced provider for <see cref="ApplyProvider"/>.
    /// </summary>
    BindingCollection<ApplyParameter, ExecutableProvider> OuterReferences { get; }

    /// <summary>
    /// Compiles the specified provider.
    /// </summary>
    /// <param name="provider">The provider to compile.</param>
    /// <returns>Compiled provider, if compiler can handle the compilation of specified provider;
    /// otherwise, <see langword="null"/>.</returns>
    ExecutableProvider Compile(CompilableProvider provider);

    /// <summary>
    /// Determines whether the <paramref name="provider"/> can be considered 
    /// as compatible with the providers produced by the current compiler.
    /// </summary>
    /// <param name="provider">The provider to check.</param>
    /// <returns>
    /// <see langword="true"/> if the specified provider is compatible; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsCompatible(ExecutableProvider provider);

    /// <summary>
    /// Wraps the specified <paramref name="provider"/>
    /// to a provider that appears as the result of compilation 
    /// by this compiler (i.e. call of <see cref="IsCompatible"/> 
    /// on the result of this method should always return <see langword="true" />).
    /// </summary>
    /// <param name="provider">The provider to wrap to a compatible provider.</param>
    /// <returns>Wrapping provider compatible with this compiler;
    /// <see langword="null"/>, if wrapping is not possible.</returns>
    ExecutableProvider ToCompatible(ExecutableProvider provider);

    /// <summary>
    /// Gets execution site location.
    /// </summary>
    Location Location { get; }
  }
}
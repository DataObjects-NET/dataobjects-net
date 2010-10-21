// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.15

using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.Compilation
{
  /// <summary>
  /// Provider compiler contract.
  /// </summary>
  /// <typeparam name="TResult">Compilation result.</typeparam>
  public interface ICompiler<TResult> : ICompiler
    where TResult : ExecutableProvider
  {
    /// <summary>
    /// Compiles the specified provider.
    /// </summary>
    /// <param name="provider">The provider to compile.</param>
    /// <returns>Compiled provider, if compiler can handle the compilation of specified provider;
    /// otherwise, <see langword="null"/>.</returns>
    new TResult Compile(CompilableProvider provider);

    /// <summary>
    /// Wraps the specified <paramref name="provider"/>
    /// to a provider that appears as the result of compilation 
    /// by this compiler (i.e. call of <see cref="ICompiler.IsCompatible"/> 
    /// on the result of this method should always return <see langword="true" />).
    /// </summary>
    /// <param name="provider">The provider to wrap to a compatible provider.</param>
    /// <returns>Wrapping provider compatible with this compiler;
    /// <see langword="null"/>, if wrapping is not possible.</returns>
    new TResult ToCompatible(ExecutableProvider provider);
  }
}
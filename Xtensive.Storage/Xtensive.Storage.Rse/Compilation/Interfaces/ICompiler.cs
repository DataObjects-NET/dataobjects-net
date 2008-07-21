// Copyright (C) 2008 Xtensive LLC.
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
  public interface ICompiler
  {
    /// <summary>
    /// Compiles the specified provider.
    /// </summary>
    /// <param name="provider">The provider to compile.</param>
    /// <returns>Compiled provider, if compiler can handle the compilation of specified provider;
    /// otherwise, <see langword="null"/>.</returns>
    ExecutableProvider Compile(Provider provider);

    /// <summary>
    /// Determines whether the <paramref name="provider"/> can be considered 
    /// as compatible with the providers produced by the current compiler.
    /// </summary>
    /// <param name="provider">The provider to check.</param>
    /// <returns>
    /// <see langword="true"/> if the specified provider is compatible; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool IsCompatible(Provider provider);

    /// <summary>
    /// Wraps the specified <paramref name="provider"/>
    /// to a provider that appears as the result of compilation 
    /// by this compiler (i.e. call of <see cref="IsCompatible"/> 
    /// on the result of this method should always return <see langword="true" />).
    /// </summary>
    /// <param name="provider">The provider to wrap to a compatible provider.</param>
    /// <returns>Wrapping provider compatible with this compiler;
    /// <see langword="null"/>, if wrapping is not possible.</returns>
    ExecutableProvider ToCompatible(Provider provider);
  }
}
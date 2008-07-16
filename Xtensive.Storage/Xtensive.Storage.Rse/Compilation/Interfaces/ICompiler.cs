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
    Provider Compile(Provider provider);
  }
}
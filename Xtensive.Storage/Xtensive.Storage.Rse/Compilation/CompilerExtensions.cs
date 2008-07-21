// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.16

using System;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.Compilation
{
  /// <summary>
  /// <see cref="ICompiler"/> related extension methods.
  /// </summary>
  public static class CompilerExtensions
  {
    /// <summary>
    /// Compiles the specified <paramref name="provider"/> by the <paramref name="compiler"/>;
    /// tries to wrap it to compatible provider (using <see cref="ICompiler.ToCompatible"/> method)
    /// on failure.
    /// </summary>
    /// <param name="compiler">The compiler to use.</param>
    /// <param name="provider">The provider to compile or wrap.</param>
    /// <param name="toCompatibleOnFailure">If set to <see langword="true"/>, the method will try 
    /// to wrap it to a compatible provider (using <see cref="ICompiler.ToCompatible"/> method)
    /// on failure.</param>
    /// <returns>A compiled provider, a compatible </returns>
    public static ExecutableProvider Compile(this ICompiler compiler, Provider provider, bool toCompatibleOnFailure)
    {
      var result = compiler.Compile(provider);
      if (result==null && toCompatibleOnFailure)
        result = compiler.ToCompatible(provider);
      return result;
    }
  }
}
// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.16

using System;
using Xtensive.Core;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.Compilation
{
  /// <summary>
  /// <see cref="ICompiler"/> related extension methods.
  /// </summary>
  public static class CompilerExtensions
  {
    /// <summary>
    /// Tries to compile the specified <paramref name="provider"/> by the <see cref="CompilationContext.CurrentCompiler"/>;
    /// if compilation by <see cref="CompilationContext.CurrentCompiler"/> fails, and <paramref name="useFallback"/>
    /// is <see langword="true" />, the compiler from the current <see cref="CompilationContext.FallbackCompiler"/>
    /// will be used to compile the <paramref name="provider"/>; wrapping to a compatible provider
    /// will be performed, if necessary.
    /// </summary>
    /// <param name="provider">The provider to compile.</param>
    /// <param name="useFallback">If set to <see langword="true"/>, current compiler
    /// from <see cref="CompilationContext.FallbackCompiler"/> will be used in case of failure.</param>
    /// <returns>
    /// A compiled provider, compatible with <see cref="CompilationContext.CurrentCompiler"/>;
    /// <see langword="null"/>, if it was impossible to produce such provider
    /// by described set of actions.
    /// </returns>
    /// <exception cref="ArgumentException"><paramref name="provider"/> is neither
    /// <see cref="CompilableProvider"/>, nor <see cref="ExecutableProvider"/>.</exception>
    public static ExecutableProvider Compile(this CompilableProvider provider, bool useFallback)
    {
      ArgumentValidator.EnsureArgumentNotNull(provider, "provider");
      var compiler = CompilationContext.CurrentCompiler;
      if (compiler == null)
        throw new InvalidOperationException();

      var fallbackCompilerBackup = CompilationContext.Current.FallbackCompiler;
      try {
        if (compiler.FallbackCompiler!=null)
          CompilationContext.Current.FallbackCompiler = compiler.FallbackCompiler;
        var ep = compiler.Compile(provider);
        if (ep!=null)
          return ep;

        if (useFallback) {
          var fallbackCompiler = CompilationContext.Current.FallbackCompiler;
          if (fallbackCompiler != null) {
            ep = fallbackCompiler.Compile(provider);
            if (ep != null)
              return compiler.IsCompatible(ep) ? ep : compiler.ToCompatible(ep);
          }
        }
      }
      finally {
        CompilationContext.Current.FallbackCompiler = fallbackCompilerBackup;
      }
      return null;
    }
  }
}
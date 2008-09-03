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
    /// if compilation by <see cref="CompilationContext.CurrentCompiler"/> fails, the compiler from the current <see cref="CompilationContext.FallbackCompiler"/>
    /// will be used to compile the <paramref name="provider"/>; wrapping to a compatible provider
    /// will be performed, if necessary.
    /// </summary>
    /// <param name="provider">The provider to compile.
    /// Current compiler from <see cref="CompilationContext.FallbackCompiler"/> will be used in case of failure.</param>
    /// <returns>
    /// A compiled provider, compatible with <see cref="CompilationContext.CurrentCompiler"/>;
    /// <see langword="null"/>, if it was impossible to produce such provider
    /// by described set of actions.
    /// </returns>
    /// <exception cref="ArgumentException"><paramref name="provider"/> is neither
    /// <see cref="CompilableProvider"/>, nor <see cref="ExecutableProvider"/>.</exception>
    public static ExecutableProvider Compile(this CompilableProvider provider)
    {
      ArgumentValidator.EnsureArgumentNotNull(provider, "provider");
      var compilationContext = CompilationContext.Current;
      if (compilationContext == null)
        throw new InvalidOperationException();

      return compilationContext.Compile(provider);
    }

    /// <summary>
    /// Tries to compile the specified <paramref name="provider"/> by the <see cref="CompilationContext.CurrentCompiler"/>
    /// if the specified <paramref name="provider"/> is <see cref="CompilableProvider"/>; otherwise does nothing.
    /// </summary>
    /// <param name="provider">The provider to compile.</param>
    /// <returns>
    /// A compiled provider, compatible with <see cref="CompilationContext.CurrentCompiler"/>;
    /// <see langword="null"/>, if it was impossible to produce such provider
    /// by described set of actions.
    /// </returns>
    /// <exception cref="ArgumentException"><paramref name="provider"/> is neither
    /// <see cref="CompilableProvider"/>, nor <see cref="ExecutableProvider"/>.</exception>
    public static ExecutableProvider Compile(this Provider provider)
    {
      return  provider as ExecutableProvider ?? Compile(provider as CompilableProvider);
    }
  }
}
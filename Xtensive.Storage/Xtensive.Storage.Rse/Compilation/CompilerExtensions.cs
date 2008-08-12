// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.16

using System;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Resources;

namespace Xtensive.Storage.Rse.Compilation
{
  /// <summary>
  /// <see cref="ICompiler"/> related extension methods.
  /// </summary>
  public static class CompilerExtensions
  {
    /// <summary>
    /// Tries to compile the specified <paramref name="provider"/> by the <paramref name="compiler"/>,
    /// if <paramref name="provider"/> is <see cref="CompilableProvider"/>;
    /// wraps it to a compatible provider (using <see cref="ICompiler.ToCompatible"/> method),
    /// if <paramref name="provider"/> is <see cref="ExecutableProvider"/>, and this is necessary;
    /// if compilation by <paramref name="compiler"/> fails, and <paramref name="fallbackToCurrentCompiler"/>
    /// is <see langword="true" />, the compiler from the current <see cref="CompilationContext"/>
    /// will be used to compile the <paramref name="provider"/>; wrapping to a compatible provider
    /// will also be performed, if necessary.
    /// </summary>
    /// <param name="compiler">The compiler to use.</param>
    /// <param name="provider">The provider to compile.</param>
    /// <param name="fallbackToCurrentCompiler">If set to <see langword="true"/>, current compiler
    /// from <see cref="CompilationContext"/> will be used in case of failure.</param>
    /// <returns>
    /// A compiled provider, compatible with <paramref name="compiler"/>;
    /// <see langword="null"/>, if it was impossible to produce such provider
    /// by described set of actions.
    /// </returns>
    /// <exception cref="ArgumentException"><paramref name="provider"/> is neither
    /// <see cref="CompilableProvider"/>, nor <see cref="ExecutableProvider"/>.</exception>
    public static ExecutableProvider Compile(this ICompiler compiler, Provider provider, bool fallbackToCurrentCompiler)
    {
      var ep = provider as ExecutableProvider;
      if (ep!=null)
        return compiler.IsCompatible(ep) ? ep : compiler.ToCompatible(ep);
      
      var cp = provider as CompilableProvider;
      if (cp==null)
        throw new ArgumentException(
          Strings.ExProviderMustBeEitherCompilableProviderOrExecutableProvider, "provider");
      
      ep = compiler.Compile(cp);
      if (ep!=null)
        return ep;
      
      if (!fallbackToCurrentCompiler)
        return null;

      var currentCompiler = CompilationContext.CurrentCompiler;
      if (currentCompiler==null)
        return null;
      
      ep = CompilationContext.Current.Compiler.Compile(cp);
      if (ep!=null)
        return compiler.IsCompatible(ep) ? ep : compiler.ToCompatible(ep);

      return null;
    }

    /// <summary>
    /// Tries to compile the specified <paramref name="provider"/> by the <paramref name="compiler"/>,
    /// if <paramref name="provider"/> is <see cref="CompilableProvider"/>;
    /// wraps it to a compatible provider (using <see cref="ICompiler.ToCompatible"/> method),
    /// if <paramref name="provider"/> is <see cref="ExecutableProvider"/>, and this is necessary;
    /// if compilation by <paramref name="compiler"/> fails, <paramref name="fallbackCompiler"/>
    /// is used to compile the <paramref name="provider"/>; wrapping to a compatible provider
    /// will also be performed, if necessary.
    /// </summary>
    /// <param name="compiler">The compiler to use.</param>
    /// <param name="provider">The provider to compile.</param>
    /// <param name="fallbackCompiler">The compiler to use, if <paramref name="compiler"/> 
    /// can't compile the provider, but it is necessary.
    /// Can be <see langword="null" />.</param>
    /// <returns>A compiled provider, compatible with <paramref name="compiler"/>;
    /// <see langword="null" />, if it was impossible to produce such provider
    /// by described set of actions.</returns>
    /// <exception cref="ArgumentException"><paramref name="provider"/> is neither 
    /// <see cref="CompilableProvider"/>, nor <see cref="ExecutableProvider"/>.</exception>
    public static ExecutableProvider Compile(this ICompiler compiler, Provider provider, ICompiler fallbackCompiler)
    {
      var ep = provider as ExecutableProvider;
      if (ep!=null)
        return compiler.IsCompatible(ep) ? ep : compiler.ToCompatible(ep);
      
      var cp = provider as CompilableProvider;
      if (cp==null)
        throw new ArgumentException(
          Strings.ExProviderMustBeEitherCompilableProviderOrExecutableProvider, "provider");
      
      ep = compiler.Compile(cp);
      if (ep!=null)
        return ep;
      
      if (fallbackCompiler==null)
        return null;

      ep = fallbackCompiler.Compile(cp);
      if (ep!=null)
        return compiler.IsCompatible(ep) ? ep : compiler.ToCompatible(ep);

      return null;
    }
  }
}
// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.04

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Providers;
using System.Linq;
using Xtensive.Storage.Rse.Resources;
using Xtensive.Core.Helpers;

namespace Xtensive.Storage.Rse.Compilation
{
  /// <summary>
  /// The context for <see cref="Provider"/> compilation.
  /// </summary>
  public class CompilationContext : Context<CompilationScope>,
    IHasExtensions
  {
    /// <summary>
    /// Gets the compiler used by <see cref="Compile"/> method of this context.
    /// </summary>
    public ICompiler Compiler { get; private set; }

    /// <inheritdoc/>
    /// <remarks>
    /// Compilation context usually must provide compiler-specific extensions,
    /// i.e. the extensions providing some information about compilation environment.
    /// </remarks>
    public IExtensionCollection Extensions { get; private set; }

    /// <summary>
    /// Compiles the specified provider by passing it to <see cref="Compiler"/>.<see cref="ICompiler.Compile"/> method.
    /// </summary>
    /// <param name="provider">The provider to compile.</param>
    /// <returns>The result of the compilation.</returns>
    /// <exception cref="InvalidOperationException">Can't compile the specified <paramref name="provider"/>.</exception>
    public Provider Compile(Provider provider)
    {
      if (provider == null)
        return null;
      var result = Compiler.Compile(provider);
      if (result==null)
        throw new InvalidOperationException(string.Format(
          Strings.ExCantCompileProviderX, provider));
      return result;
    }

    #region IContext<...> members

    /// <inheritdoc/>
    protected override CompilationScope CreateActiveScope()
    {
      return new CompilationScope(this);
    }

    /// <inheritdoc/>
    public override bool IsActive
    {
      get { return CompilationScope.CurrentContext == this; }
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="compiler"><see cref="Compiler"/> property value.</param>
    public CompilationContext(ICompiler compiler)
      : this(compiler, new ExtensionCollection())
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="compiler"><see cref="Compiler"/> property value.</param>
    /// <param name="extensions"><see cref="Extensions"/> property value.</param>
    public CompilationContext(ICompiler compiler, ExtensionCollection extensions)
    {
      Compiler   = compiler;
      Extensions = extensions;
      extensions.LockSafely(true);
    }
  }
}
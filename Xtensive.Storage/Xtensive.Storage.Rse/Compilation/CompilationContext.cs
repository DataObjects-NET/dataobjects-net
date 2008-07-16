// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.04

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers;
using System.Linq;
using Xtensive.Storage.Rse.Resources;

namespace Xtensive.Storage.Rse.Compilation
{
  /// <summary>
  /// The context of <see cref="Provider"/> compilation.
  /// </summary>
  public sealed class CompilationContext : Context<CompilationScope>
  {
    /// <summary>
    /// Gets the chain of compilers used by <see cref="Compile"/> method.
    /// </summary>
    public IEnumerable<ICompiler> Compilers { get; private set; }

    /// <summary>
    /// Compiles the specified provider by trying to sequentially 
    /// apply all the <see cref="Compilers"/> until some of them will be 
    /// able to compile the specified <paramref name="provider"/>.
    /// </summary>
    /// <param name="provider">The provider to compile.</param>
    /// <returns>The result of the compilation.</returns>
    /// <exception cref="InvalidOperationException">Compiler supporting provider <paramref name="provider"/> is not found.</exception>
    public Provider Compile(Provider provider)
    {
      if (provider == null)
        return null;
      foreach (var compiler in Compilers) {
        var result = compiler.Compile(provider);
        if (result != null)
          return result;
      }
      throw new InvalidOperationException(string.Format(
        Strings.ExCompilerNotFound, provider));
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
    /// <param name="compilers"><see cref="Compilers"/> property value.</param>
    public CompilationContext(IEnumerable<ICompiler> compilers)
    {
      Compilers = compilers;
    }
  }
}
// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.04

using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.Compilation
{
  /// <summary>
  /// Base class for any particular provider type compiler.
  /// </summary>
  public abstract class TypeCompiler
  {
    /// <summary>
    /// Gets the <see cref="Compilation.Compiler"/> this compiler is bound to.
    /// </summary>
    public Compiler Compiler { get; private set; }

    /// <summary>
    /// Compiles the specified provider.
    /// </summary>
    /// <param name="provider">The provider to compile.</param>
    /// <returns>Compiled provider.</returns>
    public abstract Provider Compile(CompilableProvider provider);


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="compiler">The <see cref="Compilation.Compiler"/> this type compiler is bound to.</param>
    protected TypeCompiler(Compiler compiler)
    {
      Compiler = compiler;
    }
  }
}
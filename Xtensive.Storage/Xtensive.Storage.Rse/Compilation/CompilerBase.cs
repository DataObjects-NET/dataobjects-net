// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.03.28

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.Compilation
{
  /// <summary>
  /// Abstract base class for RSE <see cref="Provider"/> compilers.
  /// Compiles <see cref="CompilableProvider"/>s into <see cref="ExecutableProvider"/>.
  /// </summary>
  public abstract class CompilerBase : ICompiler
  {
    /// <summary>
    /// Gets the bindings collection.
    /// </summary>
    public BindingCollection<object, ExecutableProvider> CompiledSources { get; private set; }

    /// <inheritdoc/>
    public abstract UrlInfo Location { get; }

    /// <inheritdoc/>
    public abstract ExecutableProvider Compile(CompilableProvider provider);

    /// <inheritdoc/>
    public abstract bool IsCompatible(ExecutableProvider provider);

    /// <inheritdoc/>
    public abstract ExecutableProvider ToCompatible(ExecutableProvider provider);


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="compiledSources">Bindings collection instance. Shared across all compilers.</param>
    protected CompilerBase(BindingCollection<object, ExecutableProvider> compiledSources)
    {
      CompiledSources = compiledSources;
    }
  }
}
// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.03.30

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.PreCompilation
{
  /// <summary>
  /// Composite optimizer.
  /// </summary>
  [Serializable]
  public sealed class CompositePreCompiler : IPreCompiler
  {
    private readonly IPreCompiler[] preCompilers;

    /// <inheritdoc/>
    public CompilableProvider Process(CompilableProvider rootProvider)
    {
      var provider = rootProvider;
      foreach (var optimizer in preCompilers)
        provider = optimizer.Process(provider);
      return provider;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="preCompilers">Pre-compilers to be composed.</param>
    public CompositePreCompiler(params IPreCompiler[] preCompilers)
    {
      this.preCompilers = preCompilers;
    }
  }
}
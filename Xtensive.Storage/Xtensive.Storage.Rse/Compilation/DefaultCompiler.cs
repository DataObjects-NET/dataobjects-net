// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.14

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using StoredProvider=Xtensive.Storage.Rse.Providers.Executable.StoredProvider;

namespace Xtensive.Storage.Rse.Compilation
{
  /// <summary>
  /// Default implementation of <see cref="Compiler"/>.
  /// </summary>
  [Serializable]
  public sealed class DefaultCompiler : RseCompiler
  {
    /// <inheritdoc/>
    public override bool IsCompatible(ExecutableProvider provider)
    {
      return true;
    }

    /// <inheritdoc/>
    public override ExecutableProvider ToCompatible(ExecutableProvider provider)
    {
      return new StoredProvider(new Providers.Compilable.StoredProvider(provider.Origin), provider);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitIndex(IndexProvider provider, ExecutableProvider[] sources)
    {
      throw new NotSupportedException();
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public DefaultCompiler()
      : base(RseCompiler.DefaultLocation)
    {}
  }
}
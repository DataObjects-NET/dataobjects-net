// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.14

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using StoreProvider=Xtensive.Storage.Rse.Providers.Executable.StoreProvider;

namespace Xtensive.Storage.Rse.Compilation
{
  /// <summary>
  /// Default implementation of <see cref="Compiler"/>.
  /// </summary>
  [Serializable]
  public sealed class ClientCompiler : RseCompiler
  {
    /// <inheritdoc/>
    public override bool IsCompatible(ExecutableProvider provider)
    {
      return true;
    }

    /// <inheritdoc/>
    public override ExecutableProvider ToCompatible(ExecutableProvider provider)
    {
      return new StoreProvider(new Providers.Compilable.StoreProvider(provider.Origin), provider);
    }

    /// <inheritdoc/>
    protected override ExecutableProvider VisitIndex(IndexProvider provider)
    {
      throw new NotSupportedException();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ClientCompiler(BindingCollection<object, ExecutableProvider> compiledSources)
      : base(RseCompiler.DefaultClientLocation, compiledSources)
    {}
  }
}
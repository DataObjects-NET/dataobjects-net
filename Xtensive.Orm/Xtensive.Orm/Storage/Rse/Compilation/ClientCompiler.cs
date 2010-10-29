// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.14

using System;
using Xtensive.Internals.DocTemplates;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using StoreProvider=Xtensive.Storage.Rse.Providers.Executable.StoreProvider;

namespace Xtensive.Storage.Rse.Compilation
{
  /// <summary>
  /// Default implementation of <see cref="Compiler{TResult}"/>.
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

    /// <exception cref="NotSupportedException"></exception>
    /// <inheritdoc/>
    protected override ExecutableProvider VisitIndex(IndexProvider provider)
    {
      throw new NotSupportedException(Resources.Strings.ExCurrentCompilerIsNotSuitableForThisOperationMostLikelyThereIsNoActiveSession);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ClientCompiler()
      : base(RseCompiler.DefaultClientLocation)
    {}
  }
}
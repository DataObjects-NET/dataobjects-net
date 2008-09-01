// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.08

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using System.Linq;
using RawProvider=Xtensive.Storage.Rse.Providers.Executable.RawProvider;
using SaveProvider=Xtensive.Storage.Rse.Providers.Executable.SaveProvider;

namespace Xtensive.Storage.Rse.Compilation
{
  /// <summary>
  /// Default implementation of <see cref="Compiler"/>.
  /// </summary>
  public sealed class DefaultCompiler : Compiler
  {
    /// <inheritdoc/>
    public override bool IsCompatible(ExecutableProvider provider)
    {
      return true;
    }

    /// <inheritdoc/>
    public override ExecutableProvider ToCompatible(ExecutableProvider provider)
    {
      return new SaveProvider(
        new Providers.Compilable.SaveProvider(provider.Origin, TemporaryDataScope.Enumeration, Guid.NewGuid().ToString()), 
        provider);
      // return new RawProvider(new Providers.Compilable.RawProvider(provider.Header, provider.ToArray()));
    }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary> 
    public DefaultCompiler()
    {
      FallbackCompiler = NoneCompiler;
    }
  }
}
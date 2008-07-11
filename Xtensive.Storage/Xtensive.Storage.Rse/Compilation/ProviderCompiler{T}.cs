// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.04

using System;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Declaration;

namespace Xtensive.Storage.Rse.Compilation
{
  public abstract class ProviderCompiler<TProvider> : ProviderCompiler
    where TProvider : CompilableProvider
  {
    /// <inheritdoc/>
    public sealed override Provider Compile(CompilableProvider provider)
    {
      return Compile((TProvider) provider);
    }

    protected abstract Provider Compile(TProvider provider);


    // Constructor

    protected ProviderCompiler(CompilerResolver resolver)
      : base(resolver)
    {
    }
  }
}
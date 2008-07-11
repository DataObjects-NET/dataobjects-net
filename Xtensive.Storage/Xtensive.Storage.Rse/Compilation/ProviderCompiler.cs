// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.04

using System.Diagnostics;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Declaration;

namespace Xtensive.Storage.Rse.Compilation
{
  public abstract class ProviderCompiler
  {
    private readonly CompilerResolver resolver;

    public CompilerResolver Resolver
    {
      [DebuggerHidden]
      get { return resolver; }
    }

    public abstract Provider Compile(CompilableProvider provider);


    // Constructor

    protected ProviderCompiler(CompilerResolver resolver)
    {
      this.resolver = resolver;
    }
  }
}
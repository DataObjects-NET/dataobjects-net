// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.08

using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Compilation
{
  internal sealed class ReindexProviderCompiler : TypeCompiler<ReindexProvider>
  {
    protected override ExecutableProvider Compile(ReindexProvider provider)
    {
      return new Providers.Executable.ReindexProvider(
        provider, 
        Compiler.Compile(provider.Source, true));
    }


    // Constructor

    public ReindexProviderCompiler(Compiler compiler)
      : base(compiler)
    {
    }
  }
}
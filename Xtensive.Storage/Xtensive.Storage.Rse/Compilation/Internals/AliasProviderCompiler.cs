// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.08

using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Compilation
{
  internal sealed class AliasProviderCompiler : TypeCompiler<AliasProvider>
  {
    protected override ExecutableProvider Compile(AliasProvider provider)
    {
      return new Providers.Executable.AliasProvider(
        provider,
        provider.Source.Compile(true));
    }


    // Constructor

    public AliasProviderCompiler(Compiler compiler)
      : base(compiler)
    {
    }
  }
}
// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.08

using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.Compilation
{
  internal class AliasProviderCompiler : TypeCompiler<AliasProvider>
  {
    protected override Provider Compile(AliasProvider provider)
    {
      return new Providers.Executable.AliasProvider(
        provider.Header,
        Compiler.Compile(provider.Source, true));
    }


    // Constructor

    public AliasProviderCompiler(Compiler compiler)
      : base(compiler)
    {
    }
  }
}
// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.08

using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.Compilation
{
  internal class SortProviderCompiler : TypeCompiler<SortProvider>
  {
    protected override ExecutableProvider Compile(SortProvider provider)
    {
      return new Providers.Executable.SortProvider(
        provider, 
        Compiler.Compile(provider.Source, true));
    }


    // Constructor

    public SortProviderCompiler(Compiler compiler)
      : base(compiler)
    {
    }
  }
}
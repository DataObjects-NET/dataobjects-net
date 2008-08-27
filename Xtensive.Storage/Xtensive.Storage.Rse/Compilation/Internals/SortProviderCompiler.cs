// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.08

using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Compilation
{
  internal sealed class SortProviderCompiler : TypeCompiler<SortProvider>
  {
    protected override ExecutableProvider Compile(SortProvider provider)
    {
      return new Providers.Executable.SortProvider(
        provider, 
        provider.Source.Compile());
    }


    // Constructor

    public SortProviderCompiler(Compiler compiler)
      : base(compiler)
    {
    }
  }
}
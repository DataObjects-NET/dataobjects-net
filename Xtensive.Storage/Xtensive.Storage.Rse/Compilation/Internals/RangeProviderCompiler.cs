// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.08

using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.Compilation
{
  internal class RangeProviderCompiler : TypeCompiler<RangeProvider>
  {
    protected override ExecutableProvider Compile(RangeProvider provider)
    {
      return new Providers.Executable.RangeProvider(
        provider, 
        Compiler.Compile(provider.Source, true), 
        provider.Range);
    }


    // Constructor

    public RangeProviderCompiler(Compiler compiler)
      : base(compiler)
    {
    }
  }
}
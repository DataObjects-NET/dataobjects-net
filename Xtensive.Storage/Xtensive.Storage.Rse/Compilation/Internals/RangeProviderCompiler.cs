// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.08

using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Compilation
{
  internal sealed class RangeProviderCompiler : TypeCompiler<RangeProvider>
  {
    protected override ExecutableProvider Compile(RangeProvider provider)
    {
      return new Providers.Executable.RangeProvider(
        provider,
        provider.Source.Compile());
    }


    // Constructor

    public RangeProviderCompiler(Compiler compiler)
      : base(compiler)
    {
    }
  }
}
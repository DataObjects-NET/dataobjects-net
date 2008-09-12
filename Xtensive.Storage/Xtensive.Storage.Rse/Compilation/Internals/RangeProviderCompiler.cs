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
    protected override ExecutableProvider Compile(RangeProvider provider, params ExecutableProvider[] compiledSources)
    {
      return new Providers.Executable.RangeProvider(
        provider,
        compiledSources[0]);
    }


    // Constructor

    public RangeProviderCompiler(Compiler compiler)
      : base(compiler)
    {
    }
  }
}
// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.08

using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Compilation
{
  internal sealed class FilterProviderCompiler : TypeCompiler<FilterProvider>
  {
    protected override ExecutableProvider Compile(FilterProvider provider, params ExecutableProvider[] compiledSources)
    {
      return new Providers.Executable.FilterProvider(
        provider,
        compiledSources[0]);
    }


    // Constructor

    public FilterProviderCompiler(Compiler compiler)
      : base(compiler)
    {
    }
  }
}
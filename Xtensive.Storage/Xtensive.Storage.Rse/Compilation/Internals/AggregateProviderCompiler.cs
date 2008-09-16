// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.11

using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Compilation
{
  internal class AggregateProviderCompiler : TypeCompiler<AggregateProvider>
  {
    protected override ExecutableProvider Compile(AggregateProvider provider, params ExecutableProvider[] compiledSources)
    {
      return new Providers.Executable.AggregateProvider(
        provider,
        compiledSources[0]);
    }


    // Constructor

    public AggregateProviderCompiler(Compiler compiler)
      : base(compiler)
    {
    }
  }
}
// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.09

using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Compilation
{
  internal sealed class CalculationProviderCompiler : TypeCompiler<CalculationProvider>
  {
    protected override ExecutableProvider Compile(CalculationProvider provider, params ExecutableProvider[] compiledSources)
    {
      return new Providers.Executable.CalculationProvider(
        provider,
        compiledSources[0]);
    }


    // Constructor

    public CalculationProviderCompiler(Compiler compiler)
      : base(compiler)
    {
    }
  }
}
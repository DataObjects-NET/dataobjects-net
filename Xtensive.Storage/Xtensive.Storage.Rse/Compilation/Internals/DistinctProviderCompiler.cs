// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.01.27

using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Compilation
{
  public class DistinctProviderCompiler : TypeCompiler<DistinctProvider>
  {
    protected override Providers.ExecutableProvider Compile(DistinctProvider provider, Providers.ExecutableProvider[] compiledSources)
    {
      return new Providers.Executable.DistinctProvider(
        provider,
        compiledSources[0]);
    }


    // Constructors

    public DistinctProviderCompiler(Compiler compiler)
      : base(compiler)
    {
    }
  }
}
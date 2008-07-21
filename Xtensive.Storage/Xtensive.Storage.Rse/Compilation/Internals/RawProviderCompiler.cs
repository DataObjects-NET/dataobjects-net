// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.09

using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.Compilation
{
  internal class RawProviderCompiler : TypeCompiler<RawProvider>
  {
    protected override ExecutableProvider Compile(RawProvider provider)
    {
      return new Providers.Executable.RawProvider(
        provider, 
        provider.Tuples);
    }


    // Constructor

    public RawProviderCompiler(Compiler compiler)
      : base(compiler)
    {
    }
  }
}
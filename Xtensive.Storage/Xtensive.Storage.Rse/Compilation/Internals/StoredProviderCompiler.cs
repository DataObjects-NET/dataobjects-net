// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.25

using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Compilation
{
  internal sealed class StoredProviderCompiler : TypeCompiler<StoredProvider>
  {
    protected override ExecutableProvider Compile(StoredProvider provider, params ExecutableProvider[] compiledSources)
    {
      ExecutableProvider ex = null;
      if (provider.Source != null)
        ex = compiledSources[0];
      return new Providers.Executable.StoredProvider(provider, ex);
    }


    // Constructor

    public StoredProviderCompiler(Compiler compiler)
      : base(compiler)
    {
    }
  }
}
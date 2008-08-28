// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.25

using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Compilation
{
  internal sealed class SaveProviderCompiler : TypeCompiler<SaveProvider>
  {
    protected override ExecutableProvider Compile(SaveProvider provider)
    {
      return new Providers.Executable.SaveProvider(
        provider,
        provider.Source.Compile());
    }


    // Constructor

    public SaveProviderCompiler(Compiler compiler)
      : base(compiler)
    {
    }
  }
}
// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2008.08.27

using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Compilation
{
  internal sealed class LoadProviderCompiler: TypeCompiler<LoadProvider>
  {
    protected override ExecutableProvider Compile(LoadProvider provider)
    {
      return new Providers.Executable.LoadPovider(provider);
    }


    // Constructor

    public LoadProviderCompiler(Compiler compiler)
      : base(compiler)
    {
    }
  }
}
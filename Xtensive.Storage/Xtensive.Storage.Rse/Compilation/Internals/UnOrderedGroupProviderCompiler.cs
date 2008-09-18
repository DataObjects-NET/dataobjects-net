// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.09.16

using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Compilation
{
  internal class UnOrderedGroupProviderCompiler: TypeCompiler<UnOrderedGroupProvider>
  {
    protected override ExecutableProvider Compile(UnOrderedGroupProvider provider, params ExecutableProvider[] compiledSources)
    {
      return new Providers.Executable.UnOrderedGroupProvider(
        provider,
        compiledSources[0]);
    }


    // Constructor

    public UnOrderedGroupProviderCompiler(Compiler compiler)
      : base(compiler)
    {
    }
  }
}
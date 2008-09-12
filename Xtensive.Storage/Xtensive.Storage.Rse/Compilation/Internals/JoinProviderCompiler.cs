// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.08

using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Compilation
{
  internal sealed class JoinProviderCompiler : TypeCompiler<JoinProvider>
  {
    protected override ExecutableProvider Compile(JoinProvider provider, params ExecutableProvider[] compiledSources)
    {
      return new Providers.Executable.JoinProvider(
        provider,
        compiledSources[0],
        compiledSources[1]);
    }


    // Constructor

    public JoinProviderCompiler(Compiler compiler)
      : base(compiler)
    {
    }
  }
}
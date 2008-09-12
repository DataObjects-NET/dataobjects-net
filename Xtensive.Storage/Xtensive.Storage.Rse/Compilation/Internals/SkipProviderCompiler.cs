// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.08.11

using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Compilation
{
  internal sealed class SkipProviderCompiler : TypeCompiler<SkipProvider>
  {
    protected override ExecutableProvider Compile(SkipProvider provider, params ExecutableProvider[] compiledSources)
    {
      return new Providers.Executable.SkipProvider(
        provider,
        compiledSources[0]);
    }

    // Constructor

    public SkipProviderCompiler(Compiler compiler)
      : base(compiler)
    {
    }
  }
}
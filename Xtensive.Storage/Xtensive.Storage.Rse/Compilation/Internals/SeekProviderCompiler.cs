// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.08.14

using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Compilation
{
  internal sealed class SeekProviderCompiler : TypeCompiler<SeekProvider>
  {
    protected override ExecutableProvider Compile(SeekProvider provider)
    {
      return new Providers.Executable.SeekProvider(
        provider,
        Compiler.Compile(provider.Source));
    }

    // Constructor

    public SeekProviderCompiler(Compiler compiler)
      : base(compiler)
    {
    }
  }
}
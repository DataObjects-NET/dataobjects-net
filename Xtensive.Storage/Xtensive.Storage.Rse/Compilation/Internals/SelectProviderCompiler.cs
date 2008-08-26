// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.08

using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.Compilation
{
  internal sealed class SelectProviderCompiler : TypeCompiler<SelectProvider>
  {
    protected override ExecutableProvider Compile(SelectProvider provider)
    {
      return new Providers.Executable.SelectProvider(
        provider,
        provider.Source.Compile(true), 
        provider.ColumnIndexes);
    }


    // Constructor

    public SelectProviderCompiler(Compiler compiler)
      : base(compiler)
    {
    }
  }
}
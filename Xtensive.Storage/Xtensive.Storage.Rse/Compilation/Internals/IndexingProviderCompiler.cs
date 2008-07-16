// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.08

using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.Compilation
{
  internal class IndexingProviderCompiler : TypeCompiler<IndexingProvider>
  {
    protected override Provider Compile(IndexingProvider provider)
    {
      return new Providers.Executable.IndexingProvider(
        provider.Header, 
        Compiler.Compile(provider.Source));
    }


    // Constructor

    public IndexingProviderCompiler(Compiler compiler)
      : base(compiler)
    {
    }
  }
}
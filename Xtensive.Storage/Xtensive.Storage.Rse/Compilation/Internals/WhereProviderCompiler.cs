// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.08

using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse.Compilation
{
  internal class WhereProviderCompiler : TypeCompiler<WhereProvider>
  {
    protected override Provider Compile(WhereProvider provider)
    {
      return new Providers.Executable.FilteringProvider(
        provider.Header, 
        Compiler.Compile(provider.Source, true), 
        provider.Predicate.Compile());
    }


    // Constructor

    public WhereProviderCompiler(Compiler compiler)
      : base(compiler)
    {
    }
  }
}
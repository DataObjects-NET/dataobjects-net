// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.08

using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Declaration;

namespace Xtensive.Storage.Rse.Compilation.DefaultCompilers
{
  public class WhereProviderCompiler : ProviderCompiler<WhereProvider>
  {
    protected override Provider Compile(WhereProvider provider)
    {
      return new Providers.Implementation.FilteringProvider(
        provider.Header, 
        provider.Provider.Compile(), 
        provider.Predicate.Compile());
    }


    // Constructor

    public WhereProviderCompiler(CompilerResolver resolver)
      : base(resolver)
    {
    }
  }
}
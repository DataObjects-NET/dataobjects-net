// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.08

using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Declaration;

namespace Xtensive.Storage.Rse.Compilation.DefaultCompilers
{
  public class SortProviderCompiler : ProviderCompiler<SortProvider>
  {
    protected override Provider Compile(SortProvider provider)
    {
      return new Providers.Implementation.SortProvider(
        provider.Header, 
        provider.Source.Compile(), 
        provider.TupleSortOrder);
    }


    // Constructor

    public SortProviderCompiler(CompilerResolver resolver)
      : base(resolver)
    {
    }
  }
}
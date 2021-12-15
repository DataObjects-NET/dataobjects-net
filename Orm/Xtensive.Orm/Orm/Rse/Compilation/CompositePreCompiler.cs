// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.03.30

using System.Collections.Generic;
using System.Linq;
using Xtensive.Orm.Rse.Providers;

namespace Xtensive.Orm.Rse.Compilation
{
  public sealed class CompositePreCompiler : IPreCompiler
  {
    public List<IPreCompiler> Items { get; private set; }
    public IReadOnlyList<string> Tags { get; init; }

    public CompilableProvider Process(CompilableProvider rootProvider)
    {
      var provider = rootProvider;
      if (Tags != null) {
        foreach (var tag in Tags) {
          provider = new TagProvider(provider, tag);
        }
      }

      foreach (var item in Items)
        provider = item.Process(provider);
      return provider;
    }


    // Constructors

    public CompositePreCompiler(params IPreCompiler[] preCompilers)
    {
      Items = preCompilers.ToList();
    }
  }
}

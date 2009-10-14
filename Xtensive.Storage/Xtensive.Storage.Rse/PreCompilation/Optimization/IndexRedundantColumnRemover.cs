// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.02.17

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse.Helpers;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.PreCompilation.Optimization
{
  internal sealed class IndexRedundantColumnRemover : RedundantColumnRemoverBase
  {

    protected override Provider SubstituteSelect(CompilableProvider provider)
    {
      int columnsCount = provider.Header.Length;
      List<int> value = mappings[provider];
//      var value = Merge(mappings.Value[provider], provider.Header.Order.Select(o => o.Key));
//     mappings[provider] = value;
      if (columnsCount > value.Count)
        return new SelectProvider(provider, value.ToArray());
      return provider;
    }

    // Constructors

    public IndexRedundantColumnRemover(SelectProvider originalProvider)
      : base(originalProvider)
    {
    }
  }
}
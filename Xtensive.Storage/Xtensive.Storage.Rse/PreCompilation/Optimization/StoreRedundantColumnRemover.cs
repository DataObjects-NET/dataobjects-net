// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.10.12

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.PreCompilation.Optimization
{
  [Serializable]
  internal class StoreRedundantColumnRemover : RedundantColumnRemoverBase
  {
    protected override Xtensive.Storage.Rse.Providers.Provider VisitStore(StoreProvider provider)
    {
      return base.VisitStore(provider);
    }

    protected override Provider SubstituteSelect(CompilableProvider provider)
    {
      mappings[provider] = Enumerable.Range(0, provider.Header.Length).ToList();
      return provider;
    }

    public StoreRedundantColumnRemover(SelectProvider originalProvider)
      : base(originalProvider)
    {
    }
  }
}
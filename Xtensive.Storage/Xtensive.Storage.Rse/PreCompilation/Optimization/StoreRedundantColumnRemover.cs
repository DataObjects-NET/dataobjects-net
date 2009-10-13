// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.10.12

using System;
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

    public StoreRedundantColumnRemover(SelectProvider originalProvider)
      : base(originalProvider)
    {
    }
  }
}
// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.28

using System;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Rse.PreCompilation.Correction
{
  internal sealed class ActualOrderSetter : OrderingCorrectorRewriter
  {
    protected override void OnValidateRemovingOfOrderedColumns()
    {
    }

    protected override Provider OnRemoveSortProvider(SortProvider sortProvider)
    {
      return sortProvider;
    }

    protected override CompilableProvider OnInsertSortProvider(CompilableProvider visited)
    {
      return visited;
    }


    // Constructors

    public ActualOrderSetter(Func<CompilableProvider, ProviderOrderingDescriptor> 
      orderingDescriptorResolver)
      : base(orderingDescriptorResolver)
    {
    }
  }
}
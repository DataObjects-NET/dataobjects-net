// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.04.28

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Storage.Rse.PreCompilation.Optimization;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;
using Xtensive.Storage.Rse.Resources;

namespace Xtensive.Storage.Rse.PreCompilation.Correction
{
  internal sealed class ActualOrderSetter : OrderingCorrectorRewriter
  {
    protected override void OnValidateRemovingOfOrderedColumns()
    {
    }

    protected override DirectionCollection<int> OnArrangeOrderingColumns(SelectProvider provider)
    {
      var selectOrdering = new DirectionCollection<int>();
      foreach (KeyValuePair<int, Direction> pair in provider.ExpectedOrder) {
        var columnIndex = provider.ColumnIndexes.IndexOf(pair.Key);
        if (columnIndex < 0) {
          if (selectOrdering.Count > 0)
            selectOrdering.Clear();
          break;
        }
        selectOrdering.Add(columnIndex, pair.Value);
      }
      return selectOrdering;
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
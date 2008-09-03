// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.09.01

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  public class SqlComparer
  {
    private INodeComparerProvider provider;
    private bool isInitialized;

    public CatalogComparisonResult Compare(Catalog first, Catalog second, IEnumerable<ComparisonHintBase> hints)
    {
      if (!isInitialized) {
        Initialize();
        isInitialized = true;
      }
      ArgumentValidator.EnsureArgumentNotNull(first, "first");
      ArgumentValidator.EnsureArgumentNotNull(second, "second");
      using (new ComparisonScope(new ComparisonContext(hints))) {
        NodeComparer<Catalog> catalogComparer = provider.GetNodeComparer<Catalog>();
        var result = (CatalogComparisonResult)catalogComparer.Compare(first, second);
        result.Lock(true);
        return result;
      }
    }

    protected virtual void Initialize()
    {
      provider = NodeComparerProvider.Default;
    }
  }
}
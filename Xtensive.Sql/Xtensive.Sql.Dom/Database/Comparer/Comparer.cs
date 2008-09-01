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
  public static class Comparer
  {
    public static CatalogComparisonResult Compare(Catalog catalog1, Catalog catalog2, IEnumerable<ComparisonHintBase> hints)
    {
      ArgumentValidator.EnsureArgumentNotNull(catalog1, "catalog1");
      ArgumentValidator.EnsureArgumentNotNull(catalog2, "catalog2");
      hints = hints ?? Enumerable.Empty<ComparisonHintBase>();
      throw new NotImplementedException();
    }
  }
}
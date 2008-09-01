// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.18

using System;
using System.Collections.Generic;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  internal class CollationComparer : NodeComparerBase<Collation>
  {
    public override IComparisonResult<Collation> Compare(Collation originalNode, Collation newNode)
    {
      return new CollationComparisonResult(originalNode, newNode);
    }

    public CollationComparer(INodeComparerProvider provider)
      : base(provider)
    {
    }
  }
}
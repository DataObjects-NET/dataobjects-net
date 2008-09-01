// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.27


using System.Collections.Generic;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  internal class ViewColumnComparer : NodeComparerBase<ViewColumn>
  {
    public override IComparisonResult<ViewColumn> Compare(ViewColumn originalNode, ViewColumn newNode)
    {
      return new ViewColumnComparisonResult(originalNode, newNode);
    }

    public ViewColumnComparer(INodeComparerProvider provider)
      : base(provider)
    {
    }
  }
}
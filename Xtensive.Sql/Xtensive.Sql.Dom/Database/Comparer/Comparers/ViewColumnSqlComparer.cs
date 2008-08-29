// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.27


using System.Collections.Generic;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  internal class ViewColumnSqlComparer : SqlComparerBase<ViewColumn>
  {
    public override IComparisonResult<ViewColumn> Compare(ViewColumn originalNode, ViewColumn newNode, IEnumerable<ComparisonHintBase> hints)
    {
      return new ViewColumnComparisonResult(originalNode, newNode);
    }

    public ViewColumnSqlComparer(ISqlComparerProvider provider)
      : base(provider)
    {
    }
  }
}
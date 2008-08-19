// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.19

using System.Collections.Generic;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  internal class ViewSqlComparer : WrappingSqlComparer<View, DataTableColumn, Index>
  {
    public override ComparisonResult<View> Compare(View originalNode, View newNode, IEnumerable<ComparisonHintBase> hints)
    {
      throw new System.NotImplementedException();
    }

    public ViewSqlComparer(ISqlComparerProvider provider)
      : base(provider)
    {
    }
  }
}
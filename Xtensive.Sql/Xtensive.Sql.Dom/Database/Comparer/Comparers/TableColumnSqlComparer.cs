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
  internal class TableColumnSqlComparer : WrappingSqlComparer<TableColumn, Collation>
  {
    public override ComparisonResult<TableColumn> Compare(TableColumn originalNode, TableColumn newNode, IEnumerable<ComparisonHintBase> hints)
    {
      throw new System.NotImplementedException();
    }

    public TableColumnSqlComparer(ISqlComparerProvider provider)
      : base(provider)
    {
    }
  }
}
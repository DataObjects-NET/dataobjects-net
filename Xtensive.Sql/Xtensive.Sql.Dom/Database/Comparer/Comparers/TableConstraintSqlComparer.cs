// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.18

using System.Collections.Generic;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  internal class TableConstraintSqlComparer : SqlComparerBase<TableConstraint>
  {
    public override ComparisonResult<TableConstraint> Compare(TableConstraint originalNode, TableConstraint newNode, IEnumerable<ComparisonHintBase> hints)
    {
      throw new System.NotImplementedException();
    }

    public TableConstraintSqlComparer(ISqlComparerProvider provider)
      : base(provider)
    {
    }
  }
}
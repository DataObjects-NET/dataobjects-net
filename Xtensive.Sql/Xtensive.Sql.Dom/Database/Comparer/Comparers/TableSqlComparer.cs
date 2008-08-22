// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.14

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Reflection;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  internal class TableSqlComparer : WrappingSqlComparer<Table, TableColumn, Index, TableConstraint>
  {

    public override ComparisonResult<Table> Compare(Table originalNode, Table newNode, IEnumerable<ComparisonHintBase> hints)
    {
      throw new System.NotImplementedException();
    }

    public TableSqlComparer(ISqlComparerProvider provider)
      : base(provider)
    {
    }
  }
}
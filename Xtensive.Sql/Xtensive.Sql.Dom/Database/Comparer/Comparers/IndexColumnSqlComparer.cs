// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.19

using System;
using System.Collections.Generic;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  internal class IndexColumnSqlComparer : WrappingSqlComparer<IndexColumn, DataTableColumn>
  {
    public override IComparisonResult<IndexColumn> Compare(IndexColumn originalNode, IndexColumn newNode, IEnumerable<ComparisonHintBase> hints)
    {
      IndexColumnComparisonResult result = InitializeResult<IndexColumn, IndexColumnComparisonResult>(originalNode, newNode);
      bool hasChanges = false;
      result.Ascending = CompareSimpleStruct(originalNode==null ? (bool?) null : originalNode.Ascending, newNode==null ? (bool?) null : newNode.Ascending, ref hasChanges);
      result.Column = (IComparisonResult<DataTableColumn>)BaseSqlComparer1.Compare(originalNode == null ? null : originalNode.Column, newNode == null ? null : newNode.Column, hints);
      if (hasChanges && result.ResultType == ComparisonResultType.Unchanged)
        result.ResultType = ComparisonResultType.Modified;
      result.Lock(true);
      return result;
    }

    public IndexColumnSqlComparer(ISqlComparerProvider provider)
      : base(provider)
    {
    }
  }
}
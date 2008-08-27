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
  internal class ViewSqlComparer : WrappingSqlComparer<View, ViewColumn, Index>
  {
    public override IComparisonResult<View> Compare(View originalNode, View newNode, IEnumerable<ComparisonHintBase> hints)
    {
      ViewComparisonResult result = InitializeResult<View, ViewComparisonResult>(originalNode, newNode);
      bool hasChanges = false;
      result.CheckOptions = CompareSimpleStruct(originalNode == null ? (CheckOptions?)null : originalNode.CheckOptions, newNode == null ? (CheckOptions?)null : newNode.CheckOptions, ref hasChanges);
      result.Definition = CompareSimpleNode(originalNode == null ? null : originalNode.Definition, newNode == null ? null : newNode.Definition, ref hasChanges);
      hasChanges |= CompareNestedNodes(originalNode == null ? null : originalNode.Indexes, newNode == null ? null : newNode.Indexes, hints, BaseSqlComparer2, result.Indexes);
      hasChanges |= CompareNestedNodes(originalNode == null ? null : originalNode.ViewColumns, newNode == null ? null : newNode.ViewColumns, hints, BaseSqlComparer1, result.Columns);
      if (hasChanges && result.ResultType == ComparisonResultType.Unchanged)
        result.ResultType = ComparisonResultType.Modified;
      result.Lock(true);
      return result;
    }

    public ViewSqlComparer(ISqlComparerProvider provider)
      : base(provider)
    {
    }
  }
}
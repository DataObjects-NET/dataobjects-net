// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.19

using System;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  internal class IndexColumnComparer : WrappingNodeComparer<IndexColumn, DataTableColumn>
  {
    public override IComparisonResult<IndexColumn> Compare(IndexColumn originalNode, IndexColumn newNode)
    {
      var result = ComparisonContext.Current.Factory.CreateComparisonResult<IndexColumn, IndexColumnComparisonResult>(originalNode, newNode);
      bool hasChanges = false;
      result.Ascending = CompareSimpleStruct(originalNode==null ? (bool?) null : originalNode.Ascending, newNode==null ? (bool?) null : newNode.Ascending, ref hasChanges);
      result.Column = (DataTableColumnComparisonResult) BaseNodeComparer1.Compare(originalNode==null ? null : originalNode.Column, newNode==null ? null : newNode.Column);
      if (hasChanges && result.ResultType==ComparisonResultType.Unchanged)
        result.ResultType = ComparisonResultType.Modified;
      return result;
    }

    public IndexColumnComparer(INodeComparerProvider provider)
      : base(provider)
    {
    }
  }
}
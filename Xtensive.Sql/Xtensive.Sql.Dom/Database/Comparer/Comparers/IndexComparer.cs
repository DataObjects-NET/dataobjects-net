// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.14

using System;
using System.Collections.Generic;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  internal class IndexComparer : WrappingNodeComparer<Index, IndexColumn, DataTableColumn> // TODO: Partition descriptor
  {
    public override IComparisonResult<Index> Compare(Index originalNode, Index newNode, IEnumerable<ComparisonHintBase> hints)
    {
      var result = new IndexComparisonResult(originalNode, newNode);
      bool hasChanges = false;
      result.IsUnique = CompareSimpleStruct(originalNode==null ? (bool?) null : originalNode.IsUnique, newNode==null ? (bool?) null : newNode.IsUnique, ref hasChanges);
      result.IsBitmap = CompareSimpleStruct(originalNode==null ? (bool?) null : originalNode.IsBitmap, newNode==null ? (bool?) null : newNode.IsBitmap, ref hasChanges);
      result.IsClustered = CompareSimpleStruct(originalNode==null ? (bool?) null : originalNode.IsClustered, newNode==null ? (bool?) null : newNode.IsClustered, ref hasChanges);
      result.FillFactor = CompareSimpleNode(originalNode==null ? null : originalNode.FillFactor, newNode==null ? null : newNode.FillFactor, ref hasChanges);
      result.Filegroup = CompareSimpleNode(originalNode==null ? null : originalNode.Filegroup, newNode==null ? null : newNode.Filegroup, ref hasChanges);
      hasChanges |= CompareNestedNodes(originalNode==null ? null : originalNode.Columns, newNode==null ? null : newNode.Columns, hints, BaseNodeComparer1, result.Columns);
      hasChanges |= CompareNestedNodes(originalNode==null ? null : originalNode.NonkeyColumns, newNode==null ? null : newNode.NonkeyColumns, hints, BaseNodeComparer2, result.NonkeyColumns);
      if (hasChanges && result.ResultType==ComparisonResultType.Unchanged)
        result.ResultType = ComparisonResultType.Modified;
      result.Lock(true);
      return result;
    }

    public IndexComparer(INodeComparerProvider provider)
      : base(provider)
    {
    }
  }
}
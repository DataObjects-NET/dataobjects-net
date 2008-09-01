// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.14

using System;
using System.Collections.Generic;
using Xtensive.Core.Collections;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  internal class TableComparer : WrappingNodeComparer<Table, TableColumn, Index, Constraint>
  {

    public override IComparisonResult<Table> Compare(Table originalNode, Table newNode)
    {
      var result = new TableComparisonResult(originalNode, newNode);
      bool hasChanges = false;
      result.Filegroup = CompareSimpleNode(originalNode==null ? null : originalNode.Filegroup, newNode==null ? null : newNode.Filegroup, ref hasChanges);
      hasChanges |= CompareNestedNodes(originalNode == null ? null : originalNode.Indexes, newNode == null ? null : newNode.Indexes, BaseNodeComparer2, result.Indexes);
      hasChanges |= CompareNestedNodes(originalNode == null ? null : originalNode.Columns.Convert(dataTableColumn => (TableColumn)dataTableColumn), newNode == null ? null : newNode.Columns.Convert(dataTableColumn => (TableColumn)dataTableColumn), BaseNodeComparer1, result.Columns);
      hasChanges |= CompareNestedNodes(originalNode == null ? null : ((IConstrainable)originalNode).Constraints, newNode == null ? null : ((IConstrainable)newNode).Constraints, BaseNodeComparer3, result.Constraints);
      if (hasChanges && result.ResultType==ComparisonResultType.Unchanged) {
        result.ResultType = ComparisonResultType.Modified;
      }
      return result;
    }

    public TableComparer(INodeComparerProvider provider)
      : base(provider)
    {
    }
  }
}
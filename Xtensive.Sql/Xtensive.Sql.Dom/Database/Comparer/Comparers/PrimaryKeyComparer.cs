// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.27

using System;
using System.Collections.Generic;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  [Serializable]
  internal class PrimaryKeyComparer : WrappingNodeComparer<PrimaryKey, TableColumn>
  {
    public override IComparisonResult<PrimaryKey> Compare(PrimaryKey originalNode, PrimaryKey newNode, IEnumerable<ComparisonHintBase> hints)
    {
      var result = new PrimaryKeyComparisonResult(originalNode, newNode);
      bool hasChanges = false;
      hasChanges |= CompareNestedNodes(originalNode == null ? null : originalNode.Columns, newNode == null ? null : newNode.Columns, hints, BaseNodeComparer1, result.Columns);
      if (hasChanges && result.ResultType == ComparisonResultType.Unchanged)
        result.ResultType = ComparisonResultType.Modified;
      return result;
    }

    public PrimaryKeyComparer(INodeComparerProvider provider)
      : base(provider)
    {
    }
  }
}
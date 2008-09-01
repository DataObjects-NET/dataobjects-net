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
  internal class DomainComparer : WrappingNodeComparer<Domain, DomainConstraint, Collation>
  {
    public override IComparisonResult<Domain> Compare(Domain originalNode, Domain newNode)
    {
      var result = new  DomainComparisonResult(originalNode, newNode);
      bool hasChanges = false;
      result.Collation = (CollationComparisonResult) BaseNodeComparer2.Compare(originalNode==null ? null : originalNode.Collation, newNode==null ? null : newNode.Collation);
      hasChanges |= result.Collation.HasChanges;
      result.DataType = CompareSimpleNode(originalNode == null ? null : originalNode.DataType, newNode == null ? null : newNode.DataType, ref hasChanges);
      result.DefaultValue = CompareSimpleNode(originalNode==null ? null : originalNode.DefaultValue, newNode==null ? null : newNode.DefaultValue, ref hasChanges);
      hasChanges |= CompareNestedNodes(originalNode==null ? null : originalNode.DomainConstraints, newNode==null ? null : newNode.DomainConstraints, BaseNodeComparer1, result.DomainConstraints);
      if (hasChanges && result.ResultType==ComparisonResultType.Unchanged)
        result.ResultType = ComparisonResultType.Modified;
      result.Lock(true);
      return result;
    }

    public DomainComparer(INodeComparerProvider provider)
      : base(provider)
    {
    }
  }
}
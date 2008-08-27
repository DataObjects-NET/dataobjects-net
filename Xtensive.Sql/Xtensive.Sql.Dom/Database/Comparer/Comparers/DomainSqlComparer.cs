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
  internal class DomainSqlComparer : WrappingSqlComparer<Domain, DomainConstraint, Collation>
  {
    public override IComparisonResult<Domain> Compare(Domain originalNode, Domain newNode, IEnumerable<ComparisonHintBase> hints)
    {
      DomainComparisonResult result = InitializeResult<Domain, DomainComparisonResult>(originalNode, newNode);
      bool hasChanges = false;
      result.Collation = (NodeComparisonResult<Collation>) BaseSqlComparer2.Compare(originalNode==null ? null : originalNode.Collation, newNode==null ? null : newNode.Collation, hints);
      hasChanges |= result.Collation.HasChanges;
      result.DataType = CompareSimpleNode(originalNode == null ? null : originalNode.DataType, newNode == null ? null : newNode.DataType, ref hasChanges);
      result.DefaultValue = CompareSimpleNode(originalNode==null ? null : originalNode.DefaultValue, newNode==null ? null : newNode.DefaultValue, ref hasChanges);
      hasChanges |= CompareNestedNodes(originalNode==null ? null : originalNode.DomainConstraints, newNode==null ? null : newNode.DomainConstraints, hints, BaseSqlComparer1, result.DomainConstraints);
      if (hasChanges && result.ResultType==ComparisonResultType.Unchanged)
        result.ResultType = ComparisonResultType.Modified;
      result.Lock(true);
      return result;
    }

    public DomainSqlComparer(ISqlComparerProvider provider)
      : base(provider)
    {
    }
  }
}
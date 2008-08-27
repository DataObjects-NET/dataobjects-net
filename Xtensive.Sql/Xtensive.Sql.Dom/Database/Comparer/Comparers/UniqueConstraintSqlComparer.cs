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
  internal class UniqueConstraintSqlComparer : WrappingSqlComparer<UniqueConstraint, TableColumn, PrimaryKey>
  {
    public override IComparisonResult<UniqueConstraint> Compare(UniqueConstraint originalNode, UniqueConstraint newNode, IEnumerable<ComparisonHintBase> hints)
    {
      IComparisonResult<UniqueConstraint> result;
      if (originalNode==null && newNode==null)
        result = new UniqueConstraintComparisonResult
          {
            OriginalValue = originalNode,
            NewValue = newNode,
            ResultType = ComparisonResultType.Unchanged
          };
      else if (originalNode!=null && newNode!=null && originalNode.GetType()!=newNode.GetType())
        result = new UniqueConstraintComparisonResult
          {
            OriginalValue = originalNode,
            NewValue = newNode,
            ResultType = ComparisonResultType.Modified
          };
      else if ((originalNode ?? newNode).GetType()==typeof (PrimaryKey))
        result = (IComparisonResult<UniqueConstraint>) BaseSqlComparer2.Compare(originalNode as PrimaryKey, newNode as PrimaryKey, hints);
      else {
        result = GetUnqueConstraintResult(originalNode, newNode, hints);
      }
      result.Lock(true);
      return result;
    }

    private IComparisonResult<UniqueConstraint> GetUnqueConstraintResult(UniqueConstraint originalNode, UniqueConstraint newNode, IEnumerable<ComparisonHintBase> hints)
    {
      var result = InitializeResult<UniqueConstraint, UniqueConstraintComparisonResult>(originalNode, newNode);
      bool hasChanges = false;
      hasChanges |= CompareNestedNodes(originalNode==null ? null : originalNode.Columns, newNode==null ? null : newNode.Columns, hints, BaseSqlComparer1, result.Columns);
      if (hasChanges && result.ResultType==ComparisonResultType.Unchanged)
        result.ResultType = ComparisonResultType.Modified;
      return result;
    }

    public UniqueConstraintSqlComparer(ISqlComparerProvider provider)
      : base(provider)
    {
    }
  }
}
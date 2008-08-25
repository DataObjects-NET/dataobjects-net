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
  internal class AssertionSqlComparer : SqlComparerBase<Assertion>
  {
    public override ComparisonResult<Assertion> Compare(Assertion originalNode, Assertion newNode, IEnumerable<ComparisonHintBase> hints)
    {
      AssertionComparisonResult result = InitializeResult<Assertion, AssertionComparisonResult>(originalNode, newNode);
      bool hasChanges = false;
      result.Condition = CompareSimpleNodes(originalNode.Condition, newNode.Condition);
      hasChanges |= result.Condition.HasChanges;
      result.IsDeferrable = CompareSimpleNodes(originalNode.IsDeferrable, newNode.IsDeferrable);
      hasChanges |= result.IsDeferrable.HasChanges;
      result.IsInitiallyDeferred = CompareSimpleNodes(originalNode.IsInitiallyDeferred, newNode.IsInitiallyDeferred);
      hasChanges |= result.IsInitiallyDeferred.HasChanges;
      if (hasChanges)
        result.ResultType = ComparisonResultType.Modified;
      return result;
    }

    public AssertionSqlComparer(ISqlComparerProvider provider)
      : base(provider)
    {
    }
  }
}
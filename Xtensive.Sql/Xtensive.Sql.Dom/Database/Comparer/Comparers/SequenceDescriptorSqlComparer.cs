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
  internal class SequenceDescriptorSqlComparer : SqlComparerBase<SequenceDescriptor>
  {
    public override IComparisonResult<SequenceDescriptor> Compare(SequenceDescriptor originalNode, SequenceDescriptor newNode, IEnumerable<ComparisonHintBase> hints)
    {
      var result = new SequenceDescriptorComparisonResult(originalNode, newNode);
      bool hasChanges = false;
      result.StartValue = CompareSimpleNode(originalNode == null ? null : originalNode.StartValue, newNode == null ? null : newNode.StartValue, ref hasChanges);
      result.Increment = CompareSimpleNode(originalNode == null ? null : originalNode.Increment, newNode == null ? null : newNode.Increment, ref hasChanges);
      result.MaxValue = CompareSimpleNode(originalNode == null ? null : originalNode.MaxValue, newNode == null ? null : newNode.MaxValue, ref hasChanges);
      result.MinValue = CompareSimpleNode(originalNode == null ? null : originalNode.MinValue, newNode == null ? null : newNode.MinValue, ref hasChanges);
      result.IsCyclic = CompareSimpleNode(originalNode == null ? null : originalNode.IsCyclic, newNode == null ? null : newNode.IsCyclic, ref hasChanges);
      if (hasChanges && result.ResultType == ComparisonResultType.Unchanged)
        result.ResultType = ComparisonResultType.Modified;
      result.Lock(true);
      return result;
    }

    public SequenceDescriptorSqlComparer(ISqlComparerProvider provider)
      : base(provider)
    {
    }
  }
}
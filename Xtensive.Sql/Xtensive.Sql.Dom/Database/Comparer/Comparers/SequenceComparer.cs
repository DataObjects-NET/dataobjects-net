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
  internal class SequenceComparer : WrappingNodeComparer<Sequence, SequenceDescriptor>
  {
    public override IComparisonResult<Sequence> Compare(Sequence originalNode, Sequence newNode)
    {
      var result = new SequenceComparisonResult(originalNode, newNode);
      bool hasChanges = false;
      result.DataType = CompareSimpleNode(originalNode == null ? null : originalNode.DataType, newNode == null ? null : newNode.DataType, ref hasChanges);
      result.SequenceDescriptor = (SequenceDescriptorComparisonResult) BaseNodeComparer1.Compare(originalNode == null ? null : originalNode.SequenceDescriptor, newNode == null ? null : newNode.SequenceDescriptor);
      hasChanges |= result.SequenceDescriptor.HasChanges;
      if (hasChanges && result.ResultType == ComparisonResultType.Unchanged)
        result.ResultType = ComparisonResultType.Modified;
      result.Lock(true);
      return result;
    }

    public SequenceComparer(INodeComparerProvider provider)
      : base(provider)
    {
    }
  }
}
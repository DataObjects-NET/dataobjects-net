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
  internal class SequenceSqlComparer : WrappingSqlComparer<Sequence, SequenceDescriptor>
  {
    public override IComparisonResult<Sequence> Compare(Sequence originalNode, Sequence newNode, IEnumerable<ComparisonHintBase> hints)
    {
      var result = new SequenceComparisonResult(originalNode, newNode);
      bool hasChanges = false;
      result.DataType = CompareSimpleNode(originalNode == null ? null : originalNode.DataType, newNode == null ? null : newNode.DataType, ref hasChanges);
      result.SequenceDescriptor = (SequenceDescriptorComparisonResult) BaseSqlComparer1.Compare(originalNode == null ? null : originalNode.SequenceDescriptor, newNode == null ? null : newNode.SequenceDescriptor, hints);
      hasChanges |= result.SequenceDescriptor.HasChanges;
      if (hasChanges && result.ResultType == ComparisonResultType.Unchanged)
        result.ResultType = ComparisonResultType.Modified;
      result.Lock(true);
      return result;
    }

    public SequenceSqlComparer(ISqlComparerProvider provider)
      : base(provider)
    {
    }
  }
}
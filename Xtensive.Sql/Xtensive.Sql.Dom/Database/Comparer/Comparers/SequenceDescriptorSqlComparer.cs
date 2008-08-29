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

      if (hasChanges && result.ResultType == ComparisonResultType.Unchanged)
        result.ResultType = ComparisonResultType.Modified;
      result.Lock(true);
      return result;
      // TODO: Finish comparer
//    private long? startValue;
//    private long? increment;
//    private long? maxValue;
//    private long? minValue;
//    private bool? isCyclic;
      throw new System.NotImplementedException();
    }

    public SequenceDescriptorSqlComparer(ISqlComparerProvider provider)
      : base(provider)
    {
    }
  }
}
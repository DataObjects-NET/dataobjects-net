// Copyright (C) 2007-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Nick Svetlov
// Created:    2007.11.28

using System;
using System.Runtime.Serialization;

namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class Int64Comparer : ValueTypeComparer<long>
  {
    protected override IAdvancedComparer<long> CreateNew(ComparisonRules rules)
      => new Int64Comparer(Provider, ComparisonRules.Combine(rules));


    // Constructors

    public Int64Comparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo = new ValueRangeInfo<long>(true, long.MinValue, true, long.MaxValue, true, 1);
    }

    public Int64Comparer(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
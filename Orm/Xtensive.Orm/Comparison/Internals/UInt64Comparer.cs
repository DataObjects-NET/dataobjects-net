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
  internal sealed class UInt64Comparer : ValueTypeComparer<ulong>
  {
    protected override IAdvancedComparer<ulong> CreateNew(ComparisonRules rules)
      => new UInt64Comparer(Provider, ComparisonRules.Combine(rules));


    // Constructors

    public UInt64Comparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo = new ValueRangeInfo<ulong>(true, ulong.MinValue, true, ulong.MaxValue, true, 1);
    }

    public UInt64Comparer(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
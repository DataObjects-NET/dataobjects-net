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
  internal sealed class Int32Comparer : ValueTypeComparer<int>
  {
    protected override IAdvancedComparer<int> CreateNew(ComparisonRules rules) => new Int32Comparer(Provider, ComparisonRules.Combine(rules));


    // Constructors

    public Int32Comparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo = new ValueRangeInfo<int>(true, int.MinValue, true, int.MaxValue, true, 1);
    }

    public Int32Comparer(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
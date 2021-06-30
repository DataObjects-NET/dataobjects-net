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
  internal sealed class DecimalComparer : ValueTypeComparer<decimal>
  {
    protected override IAdvancedComparer<decimal> CreateNew(ComparisonRules rules)
      => new DecimalComparer(Provider, ComparisonRules.Combine(rules));


    // Constructors

    public DecimalComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo = new ValueRangeInfo<decimal>(true, decimal.MinValue, true, decimal.MaxValue, false, 0m);
    }

    public DecimalComparer(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
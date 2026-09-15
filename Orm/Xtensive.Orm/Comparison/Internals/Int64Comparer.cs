// Copyright (C) 2007-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Nick Svetlov
// Created:    2007.11.28

using System;

namespace Xtensive.Comparison
{
  internal sealed class Int64Comparer : ValueTypeComparer<long>
  {
    protected override Int64Comparer CreateNew(ComparisonRules rules) => new(Provider, ComparisonRules.Combine(rules));


    // Constructors

    public Int64Comparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo = new ValueRangeInfo<long>(true, long.MinValue, true, long.MaxValue, true, 1);
    }
  }
}
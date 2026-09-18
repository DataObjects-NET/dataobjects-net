// Copyright (C) 2007-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Nick Svetlov
// Created:    2007.11.28

using System;

namespace Xtensive.Comparison
{
  internal sealed class Int16Comparer : ValueTypeComparer<short>
  {
    protected override Int16Comparer CreateNew(ComparisonRules rules) => new(Provider, ComparisonRules.Combine(rules));


    // Constructors

    public Int16Comparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo = new ValueRangeInfo<short>(true, short.MinValue, true, short.MaxValue, true, 1);
    }
  }
}
// Copyright (C) 2007-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Nick Svetlov
// Created:    2007.11.28

using System;

namespace Xtensive.Comparison
{
  internal sealed class DoubleComparer : ValueTypeComparer<double>
  {
    protected override DoubleComparer CreateNew(ComparisonRules rules) => new(Provider, ComparisonRules.Combine(rules));


    // Constructors

    public DoubleComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo = new ValueRangeInfo<double>(true, double.MinValue, true, double.MaxValue, false, 0d);
    }
  }
}
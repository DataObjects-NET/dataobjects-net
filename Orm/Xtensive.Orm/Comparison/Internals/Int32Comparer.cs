// Copyright (C) 2007-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Nick Svetlov
// Created:    2007.11.28

using System;
using System.Collections.Generic;

namespace Xtensive.Comparison
{
  internal sealed class Int32Comparer : ValueTypeComparer<int>
  {
    protected override Int32Comparer CreateNew(ComparisonRules rules) => new(Provider, ComparisonRules.Combine(rules));


    // Constructors

    public Int32Comparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      var abc = EqualityComparer<int>.Default;
      ValueRangeInfo = new ValueRangeInfo<int>(true, int.MinValue, true, int.MaxValue, true, 1);
    }
  }
}
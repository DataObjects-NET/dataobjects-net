// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.11.28

using System;

namespace Xtensive.Core.Comparison
{
  [Serializable]
  internal sealed class Int32Comparer : ValueTypeComparer<int>
  {
    protected override IAdvancedComparer<int> CreateNew(ComparisonRules rules)
    {
      return new Int32Comparer(Provider, ComparisonRules.Combine(rules));
    }


    // Constructors

    public Int32Comparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo = new ValueRangeInfo<int>(true, Int32.MinValue, true, Int32.MaxValue, true, 1);
    }
  }
}
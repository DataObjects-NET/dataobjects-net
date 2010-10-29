// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.11.28

using System;

namespace Xtensive.Comparison
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
      ValueRangeInfo = new ValueRangeInfo<int>(true, int.MinValue, true, int.MaxValue, true, 1);
    }
  }
}
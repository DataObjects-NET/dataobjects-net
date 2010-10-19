// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.11.28

using System;

namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class Int64Comparer : ValueTypeComparer<long>
  {
    protected override IAdvancedComparer<long> CreateNew(ComparisonRules rules)
    {
      return new Int64Comparer(Provider, ComparisonRules.Combine(rules));
    }


    // Constructors

    public Int64Comparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo = new ValueRangeInfo<long>(true, long.MinValue, true, long.MaxValue, true, 1);
    }
  }
}
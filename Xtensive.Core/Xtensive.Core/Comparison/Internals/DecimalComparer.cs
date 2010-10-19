// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.11.28

using System;

namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class DecimalComparer : ValueTypeComparer<decimal>
  {
    protected override IAdvancedComparer<decimal> CreateNew(ComparisonRules rules)
    {
      return new DecimalComparer(Provider, ComparisonRules.Combine(rules));
    }


    // Constructors

    public DecimalComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo = new ValueRangeInfo<decimal>(true, decimal.MinValue, true, decimal.MaxValue, false, 0m);
    }
  }
}
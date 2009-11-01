// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.11.28

using System;

namespace Xtensive.Core.Comparison
{
  [Serializable]
  internal sealed class DoubleComparer : ValueTypeComparer<double>
  {
    protected override IAdvancedComparer<double> CreateNew(ComparisonRules rules)
    {
      return new DoubleComparer(Provider, ComparisonRules.Combine(rules));
    }


    // Constructors

    public DoubleComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo = new ValueRangeInfo<double>(true, Double.MinValue, true, Double.MaxValue, false, 0d);
    }
  }
}
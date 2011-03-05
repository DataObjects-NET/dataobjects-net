// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.11.28

using System;

namespace Xtensive.Comparison
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
      ValueRangeInfo = new ValueRangeInfo<double>(true, double.MinValue, true, double.MaxValue, false, 0d);
    }
  }
}
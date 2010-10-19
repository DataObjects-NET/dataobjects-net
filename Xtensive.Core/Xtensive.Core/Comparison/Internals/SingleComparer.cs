// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.11.14

using System;

namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class SingleComparer : ValueTypeComparer<float>
  {
    protected override IAdvancedComparer<float> CreateNew(ComparisonRules rules)
    {
      return new SingleComparer(Provider, ComparisonRules.Combine(rules));
    }


    // Constructors

    public SingleComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo = new ValueRangeInfo<float>(true, float.MinValue, true, float.MaxValue, false, default(float));
    }
  }
}
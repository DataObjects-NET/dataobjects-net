// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.11.14

using System;

namespace Xtensive.Core.Comparison
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
      ValueRangeInfo = new ValueRangeInfo<float>(true, Single.MinValue, true, Single.MaxValue, false, default(float));
    }
  }
}
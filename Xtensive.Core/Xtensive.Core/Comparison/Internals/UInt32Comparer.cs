// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.11.28

using System;

namespace Xtensive.Core.Comparison
{
  [Serializable]
  internal sealed class UInt32Comparer : ValueTypeComparer<uint>
  {
    protected override IAdvancedComparer<uint> CreateNew(ComparisonRules rules)
    {
      return new UInt32Comparer(Provider, ComparisonRules.Combine(rules));
    }


    // Constructors

    public UInt32Comparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo = new ValueRangeInfo<uint>(true, UInt32.MinValue, true, UInt32.MaxValue, true, 1);
    }
  }
}
// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.11.28

using System;

namespace Xtensive.Comparison
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
      ValueRangeInfo = new ValueRangeInfo<uint>(true, uint.MinValue, true, uint.MaxValue, true, 1);
    }
  }
}
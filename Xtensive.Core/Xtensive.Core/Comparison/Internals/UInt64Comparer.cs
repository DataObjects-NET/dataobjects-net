// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.11.28

using System;

namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class UInt64Comparer : ValueTypeComparer<ulong>
  {
    protected override IAdvancedComparer<ulong> CreateNew(ComparisonRules rules)
    {
      return new UInt64Comparer(Provider, ComparisonRules.Combine(rules));
    }


    // Constructors

    public UInt64Comparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo = new ValueRangeInfo<ulong>(true, ulong.MinValue, true, ulong.MaxValue, true, 1);
    }
  }
}
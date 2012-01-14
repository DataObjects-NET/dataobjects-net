// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.11.28

using System;

namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class UInt16Comparer : ValueTypeComparer<ushort>
  {
    protected override IAdvancedComparer<ushort> CreateNew(ComparisonRules rules)
    {
      return new UInt16Comparer(Provider, ComparisonRules.Combine(rules));
    }


    // Constructors

    public UInt16Comparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo = new ValueRangeInfo<ushort>(true, ushort.MinValue, true, ushort.MaxValue, true, 1);
    }
  }
}
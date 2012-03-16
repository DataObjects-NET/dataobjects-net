// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.11.28

using System;

namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class SByteComparer : ValueTypeComparer<sbyte>
  {
    protected override IAdvancedComparer<sbyte> CreateNew(ComparisonRules rules)
    {
      return new SByteComparer(Provider, ComparisonRules.Combine(rules));
    }


    // Constructors

    public SByteComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo = new ValueRangeInfo<sbyte>(true, SByte.MinValue, true, SByte.MaxValue, true, 1);
    }
  }
}
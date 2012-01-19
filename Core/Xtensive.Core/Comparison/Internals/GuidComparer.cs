// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.11.28

using System;

namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class GuidComparer: ValueTypeComparer<Guid>
  {
    protected override IAdvancedComparer<Guid> CreateNew(ComparisonRules rules)
    {
      return new GuidComparer(Provider, ComparisonRules.Combine(rules));
    }


    // Constructors

    public GuidComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo = new ValueRangeInfo<Guid>(
        true, Guid.Empty,
        true, new Guid(0xFFFFFFFF, 0xFFFF, 0xFFFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF),
        false, Guid.Empty);
    }
  }
}
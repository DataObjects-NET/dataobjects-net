// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.11.28

using System;

namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class CharComparer : ValueTypeComparer<char>
  {
    protected override IAdvancedComparer<char> CreateNew(ComparisonRules rules)
    {
      return new CharComparer(Provider, ComparisonRules.Combine(rules));
    }


    // Constructors

    public CharComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo = new ValueRangeInfo<char>(true, Char.MinValue, true, Char.MaxValue, true, '\x0001');
    }
  }
}
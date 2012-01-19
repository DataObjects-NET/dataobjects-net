// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.11.28

using System;

namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class ByteComparer : ValueTypeComparer<byte>
  {
    protected override IAdvancedComparer<byte> CreateNew(ComparisonRules rules)
    {
      return new ByteComparer(Provider, ComparisonRules.Combine(rules));
    }


    // Constructors

    public ByteComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo = new ValueRangeInfo<byte>(true, Byte.MinValue, true, Byte.MaxValue, true, 1);
    }
  }
}
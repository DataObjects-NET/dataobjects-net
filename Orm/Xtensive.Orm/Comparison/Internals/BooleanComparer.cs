// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.11.28

using System;
using Xtensive.Core;


namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class BooleanComparer : ValueTypeComparer<bool>
  {
    protected override IAdvancedComparer<bool> CreateNew(ComparisonRules rules)
    {
      return new BooleanComparer(Provider, ComparisonRules.Combine(rules));
    }

    public override bool GetNearestValue(bool value, Direction direction)
    {
      if (direction==Direction.None)
        throw Exceptions.InvalidArgument(direction, "direction");
      return direction==ComparisonRules.Value.Direction ? true : false;
    }


    // Constructors

    public BooleanComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo = new ValueRangeInfo<bool>(true, false, true, true, true, true);
    }
  }
}
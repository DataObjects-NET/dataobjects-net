// Copyright (C) 2007-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Nick Svetlov
// Created:    2007.11.28

using System;
using System.Runtime.Serialization;
using Xtensive.Core;


namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class BooleanComparer : ValueTypeComparer<bool>
  {
    protected override IAdvancedComparer<bool> CreateNew(ComparisonRules rules) => new BooleanComparer(Provider, ComparisonRules.Combine(rules));

    public override bool GetNearestValue(bool value, Direction direction)
    {
      if (direction == Direction.None) {
        throw Exceptions.InvalidArgument(direction, "direction");
      }

      return direction == ComparisonRules.Value.Direction;
    }


    // Constructors

    public BooleanComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo = new ValueRangeInfo<bool>(true, false, true, true, true, true);
    }

    public BooleanComparer(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
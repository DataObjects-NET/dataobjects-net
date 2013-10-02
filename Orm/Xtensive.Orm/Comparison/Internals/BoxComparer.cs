// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.22

using System;
using Xtensive.Core;

namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class BoxComparer<T>: WrappingComparer<Box<T>, T>
  {
    protected override IAdvancedComparer<Box<T>> CreateNew(ComparisonRules rules)
    {
      return new BoxComparer<T>(Provider, ComparisonRules.Combine(rules));
    }

    public override int Compare(Box<T> x, Box<T> y)
    {
      return BaseComparer.Compare(x.Value, y.Value);
    }

    public override bool Equals(Box<T> x, Box<T> y)
    {
      return BaseComparer.Equals(x.Value, y.Value);
    }

    public override int GetHashCode(Box<T> obj)
    {
      return BaseComparer.GetHashCode(obj.Value);
    }

    public override Box<T> GetNearestValue(Box<T> value, Direction direction)
    {
      return new Box<T>(BaseComparer.GetNearestValue(value.Value, direction));
    }


    // Constructors

    public BoxComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      Box<T> minValue, maxValue, deltaValue;
      bool hasMinValue = false, hasMaxValue = false, hasDeltaValue = false;
      minValue = maxValue = deltaValue = new Box<T>(default(T));

      if (BaseComparer.ValueRangeInfo.HasMinValue) {
        minValue = new Box<T>(BaseComparer.ValueRangeInfo.MinValue);
        hasMinValue = true;
      }
      if (BaseComparer.ValueRangeInfo.HasMaxValue) {
        maxValue = new Box<T>(BaseComparer.ValueRangeInfo.MaxValue);
        hasMaxValue = true;
      }
      if (BaseComparer.ValueRangeInfo.HasDeltaValue) {
        deltaValue = new Box<T>(BaseComparer.ValueRangeInfo.DeltaValue);
        hasDeltaValue = true;
      }
      ValueRangeInfo = new ValueRangeInfo<Box<T>>(hasMinValue, minValue, hasMaxValue, maxValue, hasDeltaValue, deltaValue);
    }
  }
}

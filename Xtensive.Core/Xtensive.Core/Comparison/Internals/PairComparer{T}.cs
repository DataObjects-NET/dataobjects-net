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
  internal sealed class PairComparer<T>: WrappingComparer<Pair<T>, T>
  {
    protected override IAdvancedComparer<Pair<T>> CreateNew(ComparisonRules rules)
    {
      return new PairComparer<T>(Provider, ComparisonRules.Combine(rules));
    }

    public override int Compare(Pair<T> x, Pair<T> y)
    {
      int result = BaseComparer.Compare(x.First, y.First);
      if (result != 0)
        return result;
      return BaseComparer.Compare(x.Second, y.Second);
    }

    public override bool Equals(Pair<T> x, Pair<T> y)
    {
      if (!BaseComparer.Equals(x.First, y.First))
        return false;
      return BaseComparer.Equals(x.Second, y.Second);
    }

    public override int GetHashCode(Pair<T> obj)
    {
      int result = BaseComparer.GetHashCode(obj.First);
      return result ^ BaseComparer.GetHashCode(obj.Second);
    }

    public override Pair<T> GetNearestValue(Pair<T> value, Direction direction)
    {
      return new Pair<T>(value.First, BaseComparer.GetNearestValue(value.Second, direction));
    }


    // Constructors

    public PairComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      Pair<T> minValue, maxValue, deltaValue;
      bool hasMinValue = false, hasMaxValue = false, hasDeltaValue = false;
      minValue = maxValue = deltaValue = new Pair<T>(default(T), default(T));

      if (BaseComparer.ValueRangeInfo.HasMinValue) {
        minValue = new Pair<T>(BaseComparer.ValueRangeInfo.MinValue, BaseComparer.ValueRangeInfo.MinValue);
        hasMinValue = true;
      }
      if (BaseComparer.ValueRangeInfo.HasMaxValue) {
        maxValue = new Pair<T>(BaseComparer.ValueRangeInfo.MaxValue, BaseComparer.ValueRangeInfo.MaxValue);
        hasMaxValue = true;
      }
      if (BaseComparer.ValueRangeInfo.HasDeltaValue) {
        deltaValue = new Pair<T>(BaseComparer.ValueRangeInfo.DeltaValue, BaseComparer.ValueRangeInfo.DeltaValue);
        hasDeltaValue = true;
      }
      ValueRangeInfo = new ValueRangeInfo<Pair<T>>(hasMinValue, minValue, hasMaxValue, maxValue, hasDeltaValue, deltaValue);
    }
  }
}

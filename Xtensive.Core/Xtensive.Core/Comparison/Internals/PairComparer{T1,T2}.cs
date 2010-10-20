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
  internal sealed class PairComparer<T1, T2>: WrappingComparer<Pair<T1, T2>, T1, T2>
  {
    protected override IAdvancedComparer<Pair<T1, T2>> CreateNew(ComparisonRules rules)
    {
      return new PairComparer<T1, T2>(Provider, ComparisonRules.Combine(rules));
    }

    public override int Compare(Pair<T1, T2> x, Pair<T1, T2> y)
    {
      int result = BaseComparer1.Compare(x.First, y.First);
      if (result != 0)
        return result;
      return BaseComparer2.Compare(x.Second, y.Second);
    }

    public override bool Equals(Pair<T1, T2> x, Pair<T1, T2> y)
    {
      if (!BaseComparer1.Equals(x.First, y.First))
        return false;
      return BaseComparer2.Equals(x.Second, y.Second);
    }

    public override int GetHashCode(Pair<T1, T2> obj)
    {
      int result = BaseComparer1.GetHashCode(obj.First);
      return result ^ BaseComparer2.GetHashCode(obj.Second);
    }

    public override Pair<T1, T2> GetNearestValue(Pair<T1, T2> value, Direction direction)
    {
      return new Pair<T1, T2>(value.First, BaseComparer2.GetNearestValue(value.Second, direction));
    }


    // Constructors

    public PairComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      Pair<T1, T2> minValue, maxValue, deltaValue;
      bool hasMinValue = false, hasMaxValue = false, hasDeltaValue = false;
      minValue = maxValue = deltaValue = new Pair<T1, T2>(default(T1), default(T2));

      if (BaseComparer1.ValueRangeInfo.HasMinValue & BaseComparer2.ValueRangeInfo.HasMinValue) {
        minValue = new Pair<T1, T2>(BaseComparer1.ValueRangeInfo.MinValue, BaseComparer2.ValueRangeInfo.MinValue);
        hasMinValue = true;
      }
      if (BaseComparer1.ValueRangeInfo.HasMaxValue & BaseComparer2.ValueRangeInfo.HasMaxValue) {
        maxValue = new Pair<T1, T2>(BaseComparer1.ValueRangeInfo.MaxValue, BaseComparer2.ValueRangeInfo.MaxValue);
        hasMaxValue = true;
      }
      if (BaseComparer1.ValueRangeInfo.HasDeltaValue & BaseComparer2.ValueRangeInfo.HasDeltaValue) {
        deltaValue = new Pair<T1, T2>(BaseComparer1.ValueRangeInfo.DeltaValue, BaseComparer2.ValueRangeInfo.DeltaValue);
        hasDeltaValue = true;
      }
      ValueRangeInfo = new ValueRangeInfo<Pair<T1, T2>>(hasMinValue, minValue, hasMaxValue, maxValue, hasDeltaValue, deltaValue);
    }
  }
}

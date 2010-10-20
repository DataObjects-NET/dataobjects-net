// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.22

using System;
using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class KeyValuePairComparer<T1, T2>: WrappingComparer<KeyValuePair<T1, T2>, T1, T2>,
    ISystemComparer<KeyValuePair<T1, T2>>
  {
    protected override IAdvancedComparer<KeyValuePair<T1, T2>> CreateNew(ComparisonRules rules)
    {
      return new KeyValuePairComparer<T1, T2>(Provider, ComparisonRules.Combine(rules));
    }

    public override int Compare(KeyValuePair<T1, T2> x, KeyValuePair<T1, T2> y)
    {
      int result = BaseComparer1.Compare(x.Key, y.Key);
      if (result != 0)
        return result;
      return BaseComparer2.Compare(x.Value, y.Value);
    }

    public override bool Equals(KeyValuePair<T1, T2> x, KeyValuePair<T1, T2> y)
    {
      if (!BaseComparer1.Equals(x.Key, y.Key))
        return false;
      return BaseComparer2.Equals(x.Value, y.Value);
    }

    public override int GetHashCode(KeyValuePair<T1, T2> obj)
    {
      int result = BaseComparer1.GetHashCode(obj.Key);
      return result ^ BaseComparer2.GetHashCode(obj.Value);
    }

    public override KeyValuePair<T1, T2> GetNearestValue(KeyValuePair<T1, T2> value, Direction direction)
    {
      return new KeyValuePair<T1, T2>(value.Key, BaseComparer2.GetNearestValue(value.Value, direction));
    }


    // Constructors

    public KeyValuePairComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      KeyValuePair<T1, T2> minValue, maxValue, deltaValue;
      bool hasMinValue = false, hasMaxValue = false, hasDeltaValue = false;
      minValue = maxValue = deltaValue = new KeyValuePair<T1, T2>(default(T1), default(T2));

      if (BaseComparer1.ValueRangeInfo.HasMinValue & BaseComparer2.ValueRangeInfo.HasMinValue) {
        minValue = new KeyValuePair<T1, T2>(BaseComparer1.ValueRangeInfo.MinValue, BaseComparer2.ValueRangeInfo.MinValue);
        hasMinValue = true;
      }
      if (BaseComparer1.ValueRangeInfo.HasMaxValue & BaseComparer2.ValueRangeInfo.HasMaxValue) {
        maxValue = new KeyValuePair<T1, T2>(BaseComparer1.ValueRangeInfo.MaxValue, BaseComparer2.ValueRangeInfo.MaxValue);
        hasMaxValue = true;
      }
      if (BaseComparer1.ValueRangeInfo.HasDeltaValue & BaseComparer2.ValueRangeInfo.HasDeltaValue) {
        deltaValue = new KeyValuePair<T1, T2>(BaseComparer1.ValueRangeInfo.DeltaValue, BaseComparer2.ValueRangeInfo.DeltaValue);
        hasDeltaValue = true;
      }
      ValueRangeInfo = new ValueRangeInfo<KeyValuePair<T1, T2>>(hasMinValue, minValue, hasMaxValue, maxValue, hasDeltaValue, deltaValue);
    }
  }
}

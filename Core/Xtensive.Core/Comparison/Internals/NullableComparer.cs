// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2008.01.23

using System;
using Xtensive.Core;

namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class NullableComparer<T>: WrappingComparer<T?, T>
    where T: struct
  {
    [NonSerialized]
    private Func<T, T, int>  currentBaseCompare;
    [NonSerialized]
    private Predicate<T, T>  currentBaseEquals;
    [NonSerialized]
    private Func<T, int>     currentBaseGetHashCode;


    protected override IAdvancedComparer<T?> CreateNew(ComparisonRules rules)
    {
      return new NullableComparer<T>(Provider, ComparisonRules.Combine(rules));
    }

    public override int Compare(T? x, T? y)
    {
      if (!x.HasValue) {
        if (!y.HasValue)
          return 0;
        else
          return -DefaultDirectionMultiplier;
      }
      else {
        if (!y.HasValue)
          return DefaultDirectionMultiplier;
        else
          return currentBaseCompare(x.GetValueOrDefault(), y.GetValueOrDefault());
      }
    }

    public override bool Equals(T? x, T? y)
    {
      if (!x.HasValue) {
        if (!y.HasValue)
          return true;
        else
          return false;
      }
      else {
        if (!y.HasValue)
          return false;
        else
          return currentBaseEquals(x.GetValueOrDefault(), y.GetValueOrDefault());
      }
    }

    public override int GetHashCode(T? obj)
    {
      if (!obj.HasValue)
        return 0;
      return currentBaseGetHashCode(obj.GetValueOrDefault());
    }


    public override T? GetNearestValue(T? value, Direction direction)
    {
      if (direction == Direction.None)
        throw Exceptions.InvalidArgument(direction, "direction");
      if (!value.HasValue)
        return value;
      if (direction!=ComparisonRules.Value.Direction) { // Opposite direction
        if (BaseComparer.ValueRangeInfo.HasMinValue && Equals(value.GetValueOrDefault(), BaseComparer.ValueRangeInfo.MinValue))
          return null;
      }
      return BaseComparer.GetNearestValue(value.GetValueOrDefault(), direction);
    }

    private void Initialize()
    {
      ValueRangeInfo<T> baseValueRangeInfo = BaseComparer.ValueRangeInfo;
      ValueRangeInfo =
        new ValueRangeInfo<T?>(
          true, null,
          baseValueRangeInfo.HasMaxValue,
          baseValueRangeInfo.HasMaxValue ? baseValueRangeInfo.MaxValue : default(T),
          baseValueRangeInfo.HasDeltaValue,
          baseValueRangeInfo.HasDeltaValue ? baseValueRangeInfo.DeltaValue : default(T));

      currentBaseCompare     = BaseComparer.Compare;
      currentBaseEquals      = BaseComparer.Equals;
      currentBaseGetHashCode = BaseComparer.GetHashCode;
    }


    // Constructors

    public NullableComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      Initialize();
    }

    public override void OnDeserialization(object sender)
    {
      base.OnDeserialization(sender);
      Initialize();
    }
  }
}

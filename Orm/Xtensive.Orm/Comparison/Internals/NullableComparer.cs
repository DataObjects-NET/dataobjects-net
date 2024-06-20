// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: 
// Created:    2008.01.23

using System;
using System.Runtime.Serialization;
using System.Security;
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


    protected override NullableComparer<T> CreateNew(ComparisonRules rules)
      => new NullableComparer<T>(Provider, ComparisonRules.Combine(rules));

    public override int Compare(T? x, T? y)
    {
      if (!x.HasValue) {
        return !y.HasValue ? 0 : -DefaultDirectionMultiplier;
      }
      else {
        return !y.HasValue
          ? DefaultDirectionMultiplier
          : currentBaseCompare(x.GetValueOrDefault(), y.GetValueOrDefault());
      }
    }

    public override bool Equals(T? x, T? y)
    {
      if (!x.HasValue) {
        return !y.HasValue;
      }
      else {
        return y.HasValue && currentBaseEquals(x.GetValueOrDefault(), y.GetValueOrDefault());
      }
    }

    public override int GetHashCode(T? obj)
      => !obj.HasValue ? 0 : currentBaseGetHashCode(obj.GetValueOrDefault());


    public override T? GetNearestValue(T? value, Direction direction)
    {
      if (direction == Direction.None) {
        throw Exceptions.InvalidArgument(direction, "direction");
      }
      if (!value.HasValue) {
        return value;
      }
      if (direction!=ComparisonRules.Value.Direction) { // Opposite direction
        if (BaseComparer.ValueRangeInfo.HasMinValue && Equals(value.GetValueOrDefault(), BaseComparer.ValueRangeInfo.MinValue)) {
          return null;
        }
      }
      return BaseComparer.GetNearestValue(value.GetValueOrDefault(), direction);
    }

    private void Initialize()
    {
      var baseValueRangeInfo = BaseComparer.ValueRangeInfo;
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

    public NullableComparer(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    public override void OnDeserialization(object sender)
    {
      base.OnDeserialization(sender);
      Initialize();
    }
  }
}

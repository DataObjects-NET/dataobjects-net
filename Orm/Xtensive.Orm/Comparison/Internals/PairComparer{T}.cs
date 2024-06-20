// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.01.22

using System;
using System.Runtime.Serialization;
using Xtensive.Core;

namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class PairComparer<T>: WrappingComparer<Pair<T>, T>
  {
    protected override PairComparer<T> CreateNew(ComparisonRules rules)
      => new PairComparer<T>(Provider, ComparisonRules.Combine(rules));

    public override int Compare(Pair<T> x, Pair<T> y)
    {
      var result = BaseComparer.Compare(x.First, y.First);
      return result != 0 ? result : BaseComparer.Compare(x.Second, y.Second);
    }

    public override bool Equals(Pair<T> x, Pair<T> y)
      => BaseComparer.Equals(x.First, y.First) && BaseComparer.Equals(x.Second, y.Second);

    public override int GetHashCode(Pair<T> obj)
    {
      var result = BaseComparer.GetHashCode(obj.First);
      return result ^ BaseComparer.GetHashCode(obj.Second);
    }

    public override Pair<T> GetNearestValue(Pair<T> value, Direction direction)
      => new Pair<T>(value.First, BaseComparer.GetNearestValue(value.Second, direction));

    private void Initialize()
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


    // Constructors

    public PairComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      Initialize();
    }

    public PairComparer(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      Initialize();
    }
  }
}

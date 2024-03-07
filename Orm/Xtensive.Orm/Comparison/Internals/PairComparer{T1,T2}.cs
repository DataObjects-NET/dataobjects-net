// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.01.22

using System.Runtime.Serialization;
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
      var result = BaseComparer1.Compare(x.First, y.First);
      return result != 0 ? result : BaseComparer2.Compare(x.Second, y.Second);
    }

    public override bool Equals(Pair<T1, T2> x, Pair<T1, T2> y)
      => BaseComparer1.Equals(x.First, y.First) && BaseComparer2.Equals(x.Second, y.Second);

    public override int GetHashCode(Pair<T1, T2> obj)
    {
      var result = BaseComparer1.GetHashCode(obj.First);
      return result ^ BaseComparer2.GetHashCode(obj.Second);
    }

    public override Pair<T1, T2> GetNearestValue(Pair<T1, T2> value, Direction direction)
      => new Pair<T1, T2>(value.First, BaseComparer2.GetNearestValue(value.Second, direction));

    private void Initialize()
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

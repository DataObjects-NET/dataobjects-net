// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.01

using System;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Indexing
{
  [Serializable]
  internal sealed class TupleEntireComparer<T>: WrappingComparer<Entire<T>, T>,
    IComparer<Entire<T>, T>,
    ISystemComparer<Entire<T>> where T : Tuple
  {
    private static readonly int[,] nearestMatrix = new[,] {
      { -1, -1,  0 }, 
      { -1,  0,  1 }, 
      {  0,  1,  1 }, 
    };

    public override Func<Entire<T>, TSecond, int> GetAsymmetric<TSecond>()
    {
      Type type = typeof(TSecond);
      if (typeof(T) != type)
        throw new NotSupportedException();
      return ((IComparer<Entire<T>, TSecond>)(object)this).Compare;
    }

    public int Compare(Entire<T> x, T y)
    {
      if (!x.HasValue)
        return (int) x.ValueType * DefaultDirectionMultiplier;
      if (y == null)
        return DefaultDirectionMultiplier;

      int result = BaseComparer.Compare(x.Value, y);
      var xValueCount = x.Value.Count;
      if (result != 0) {
        var itemIndex = result>=0 ? result : -result;
          if (itemIndex == xValueCount + 1)
            if (x.ValueType != EntireValueType.Exact)
              return (int) x.ValueType * DefaultDirectionMultiplier * (int) ComparisonRules[itemIndex - 2].Value.Direction;
        return result;
      }

      var countDelta = xValueCount - y.Count;
      if (countDelta == 0)
        return (int) x.ValueType * DefaultDirectionMultiplier * (int) ComparisonRules[xValueCount - 1].Value.Direction;
      if (countDelta < 0)
        return (int) x.ValueType * DefaultDirectionMultiplier * (int) ComparisonRules[xValueCount - 1].Value.Direction;
      return result;
    }

    public override int Compare(Entire<T> x, Entire<T> y)
    {
      int valueTypesComparison = (x.ValueType - y.ValueType);
      if (!x.HasValue || !y.HasValue)
        return valueTypesComparison * DefaultDirectionMultiplier;

      int xValueCount = x.Value.Count;
      int yValueCount = y.Value.Count;
      int countDelta = xValueCount - yValueCount;
      int result = BaseComparer.Compare(x.Value, y.Value);

      if (result != 0) {
        int itemIndex = result >= 0 ? result : -result;
        if (itemIndex==xValueCount + 1)
          if (x.ValueType!=EntireValueType.Exact)
            return (int) x.ValueType * DefaultDirectionMultiplier * (int) ComparisonRules[itemIndex - 2].Value.Direction;
        if (itemIndex==yValueCount + 1)
          if (y.ValueType!=EntireValueType.Exact)
            return -(int) y.ValueType * DefaultDirectionMultiplier * (int) ComparisonRules[itemIndex - 2].Value.Direction;
        return result;
      }

      if (countDelta == 0)
        return valueTypesComparison * DefaultDirectionMultiplier * (int) ComparisonRules[xValueCount - 1].Value.Direction;
      if (countDelta < 0)
        return (int) x.ValueType * DefaultDirectionMultiplier * (int) ComparisonRules[xValueCount - 1].Value.Direction;
      return (int) y.ValueType * DefaultDirectionMultiplier * (int) ComparisonRules[yValueCount - 1].Value.Direction;
    }

    public override bool Equals(Entire<T> x, Entire<T> y)
    {
      return Compare(x, y) == 0;
    }

    public override int GetHashCode(Entire<T> obj)
    {
      return AdvancedComparerStruct<Entire<T>>.System.GetHashCode(obj);
    }

    protected override IAdvancedComparer<Entire<T>> CreateNew(ComparisonRules rules)
    {
      return new TupleEntireComparer<T>(Provider, ComparisonRules.Combine(rules));
    }

    public override Entire<T> GetNearestValue(Entire<T> value, Direction direction)
    {
      int d = (int)direction * DefaultDirectionMultiplier;
      if (d == 0 || !value.HasValue)
        return value;
      return new Entire<T>(value.value, (Direction)nearestMatrix[1 + (int)value.valueType, 1 + d]);
    }


    // Constructor

    public TupleEntireComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo<T> baseValueRangeInfo = BaseComparer.ValueRangeInfo;
      Entire<T> min = Entire<T>.MinValue;
      Entire<T> max = Entire<T>.MaxValue;
      ValueRangeInfo = new ValueRangeInfo<Entire<T>>(
        true, min,
        true, max,
        baseValueRangeInfo.HasDeltaValue,
        baseValueRangeInfo.HasDeltaValue ? new Entire<T>(baseValueRangeInfo.DeltaValue) : new Entire<T>(default(T)));
    }
  }
}
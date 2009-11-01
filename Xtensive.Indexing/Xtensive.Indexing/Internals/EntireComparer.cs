// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2008.01.23

using System;
using Xtensive.Core;
using Xtensive.Core.Comparison;

namespace Xtensive.Indexing
{
  [Serializable]
  internal sealed class EntireComparer<T>: WrappingComparer<Entire<T>, T>,
    IComparer<Entire<T>, T>, ISystemComparer<Entire<T>>
  {
    private static readonly int[,] nearestMatrix = new int[,] {
      { -1, -1,  0 }, 
      { -1,  0,  1 }, 
      {  0,  1,  1 }, 
    };

    protected override IAdvancedComparer<Entire<T>> CreateNew(ComparisonRules rules)
    {
      return new EntireComparer<T>(Provider, ComparisonRules.Combine(rules));
    }

    public override Func<Entire<T>, TSecond, int> GetAsymmetric<TSecond>()
    {
      Type type = typeof(TSecond);
      if (typeof(T)!=type)
        throw new NotSupportedException();
      return ((IComparer<Entire<T>, TSecond>)(object)this).Compare;
    }

    public override int Compare(Entire<T> x, Entire<T> y)
    {
      if (x.HasValue && y.HasValue) {
        int r = BaseComparer.Compare(x.Value, y.Value);
        if (r!=0)
          return r;
      }
      return ((int)x.valueType - (int)y.valueType) * DefaultDirectionMultiplier * (int)ComparisonRules[0].Value.Direction;
    }

    public int Compare(Entire<T> x, T y)
    {
      switch (x.valueType) {
      case EntireValueType.NegativeInfinity:
      case EntireValueType.PositiveInfinity:
        return (int)x.valueType * DefaultDirectionMultiplier * (int)ComparisonRules[0].Value.Direction;
      case EntireValueType.NegativeInfinitesimal:
      case EntireValueType.PositiveInfinitesimal:
        int result = BaseComparer.Compare(x.value, y);
        return result!=0 ? result : (int) x.valueType * DefaultDirectionMultiplier * (int)ComparisonRules[0].Value.Direction;
      case EntireValueType.Exact:
        return BaseComparer.Compare(x.value, y);
      default:
        throw Exceptions.InternalError("Unknown EntireValueType.", Log.Instance);
      }
    }

    public override bool Equals(Entire<T> x, Entire<T> y)
    {
      EntireValueType xEntireValueType = x.ValueType;
      if (xEntireValueType != y.ValueType)
        return false;
      if (xEntireValueType != EntireValueType.Exact)
        return true;
      return BaseComparer.Equals(x.Value, y.Value);
    }

    public override int GetHashCode(Entire<T> obj)
    {
      return AdvancedComparerStruct<Entire<T>>.System.GetHashCode(obj);
    }

    public override Entire<T> GetNearestValue(Entire<T> value, Direction direction)
    {
      int d = (int) direction * DefaultDirectionMultiplier * (int)ComparisonRules[0].Value.Direction;
      if (d==0 || !value.HasValue)
        return value;
      else
        return new Entire<T>(value.value, (Direction) nearestMatrix[1 + (int)value.valueType, 1 + d]);
    }


    // Constructors

    public EntireComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo<T> baseValueRangeInfo = BaseComparer.ValueRangeInfo;
      Entire<T> min = (Entire<T>) Entire<T>.MinValue;
      Entire<T> max = (Entire<T>) Entire<T>.MaxValue;
      ValueRangeInfo = new ValueRangeInfo<Entire<T>>(
        true, min, 
        true, max, 
        baseValueRangeInfo.HasDeltaValue, 
        baseValueRangeInfo.HasDeltaValue ? (Entire<T>)Entire<T>.Create(baseValueRangeInfo.DeltaValue) : (Entire<T>)Entire<T>.Create(default(T)));
    }
  }
}

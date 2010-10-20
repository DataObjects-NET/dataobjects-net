// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2008.01.23

using System;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Indexing
{
  [Serializable]
  internal sealed class EntireComparer<T>: WrappingComparer<Entire<T>, T>,
    IComparer<Entire<T>, T>, 
    ISystemComparer<Entire<T>>,
    ISubstitutable<IAdvancedComparer<Entire<T>>>
  {
    private readonly IAdvancedComparer<Entire<T>> substitution;
    private static readonly int[,] nearestMatrix = new[,] {
      { -1, -1,  0 }, 
      { -1,  0,  1 }, 
      {  0,  1,  1 }, 
    };

    IAdvancedComparer<Entire<T>> ISubstitutable<IAdvancedComparer<Entire<T>>>.Substitution
    {
      get { return substitution; }
    }

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
      return ((int)x.valueType - (int)y.valueType) * DefaultDirectionMultiplier;
    }

    public int Compare(Entire<T> x, T y)
    {
      switch (x.valueType) {
        case EntireValueType.NegativeInfinity:
        case EntireValueType.PositiveInfinity:
          return (int)x.valueType * DefaultDirectionMultiplier;
        case EntireValueType.NegativeInfinitesimal:
        case EntireValueType.PositiveInfinitesimal:
          int result = BaseComparer.Compare(x.value, y);
          return result!=0 ? result : (int) x.valueType * DefaultDirectionMultiplier;
        case EntireValueType.Exact:
          return BaseComparer.Compare(x.value, y);
        default:
          throw Exceptions.InternalError("Unknown EntireValueType.", Log.Instance);
      }
    }

    public override bool Equals(Entire<T> x, Entire<T> y)
    {
      if (x.HasValue ^ y.HasValue)
        return false;
      EntireValueType xEntireValueType = x.ValueType;
      if (xEntireValueType != y.ValueType)
        return false;
      return BaseComparer.Equals(x.Value, y.Value);
    }

    public override int GetHashCode(Entire<T> obj)
    {
      return AdvancedComparerStruct<Entire<T>>.System.GetHashCode(obj);
    }

    public override Entire<T> GetNearestValue(Entire<T> value, Direction direction)
    {
      int d = (int) direction * DefaultDirectionMultiplier;
      if (d==0 || !value.HasValue)
        return value;
      return new Entire<T>(value.value, (Direction) nearestMatrix[1 + (int)value.valueType, 1 + d]);
    }


    // Constructors

    public EntireComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      if (typeof(Tuple).IsAssignableFrom(typeof(T))) {
        var tupleEntireComparer = typeof (TupleEntireComparer<>).Activate(new[] {typeof (T)}, new object[] {provider, comparisonRules});
        substitution = tupleEntireComparer as IAdvancedComparer<Entire<T>>;
      }
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

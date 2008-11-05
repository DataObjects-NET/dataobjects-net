// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.01

using System;
using Xtensive.Core;
using Xtensive.Core.Comparison;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Threading;
using Xtensive.Core.Tuples;

namespace Xtensive.Indexing
{
  [Serializable]
  internal sealed class TupleEntireComparer<T>: WrappingComparer<Entire<T>, T>,
    IComparer<Entire<T>, T>,
    ISystemComparer<Entire<T>> where T : Tuple
  {
    public override Func<Entire<T>, TSecond, int> GetAsymmetric<TSecond>()
    {
      Type type = typeof(TSecond);
      if (typeof(T) != type)
        throw new NotSupportedException();
      return ((IComparer<Entire<T>, TSecond>)(object)this).Compare;
    }

    public int Compare(Entire<T> x, T y)
    {
      if (x.Value == null) {
        if (y == null)
          return (int)x.ValueType * DefaultDirectionMultiplier;
        if (x.ValueType.IsInfinity())
          return (int) x.ValueType * DefaultDirectionMultiplier;
        return -DefaultDirectionMultiplier;
      }
      if (y == null)
        return x.ValueType==EntireValueType.NegativeInfinitesimal ? DefaultDirectionMultiplier : (int) x.ValueType * DefaultDirectionMultiplier;

      var valuesComparison = BaseComparer.Compare(x.Value, y);
      if (valuesComparison != 0) {
        var itemIndex = Math.Abs(valuesComparison);
        if (itemIndex==x.Value.Count + 1)
          if (x.ValueType!=EntireValueType.Exact)
            return (int) x.ValueType * DefaultDirectionMultiplier * (int) ComparisonRules[itemIndex - 2].Value.Direction;
        if (itemIndex == x.Value.Count) {
          if (itemIndex == x.Value.Count)
            if (x.ValueType.IsInfinity())
              return (int)x.ValueType * DefaultDirectionMultiplier * (int)ComparisonRules[x.Value.Count - 1].Value.Direction;
        }
        return valuesComparison;
      }

      var countDiff = x.Value.Count - y.Count;
      if (countDiff == 0)
        return (int)x.ValueType * DefaultDirectionMultiplier * (int)ComparisonRules[x.Value.Count - 1].Value.Direction;
      if (countDiff < 0)
        return (int)x.ValueType * DefaultDirectionMultiplier * (int)ComparisonRules[x.Value.Count - 1].Value.Direction;
      return valuesComparison;
    }

    public override int Compare(Entire<T> x, Entire<T> y)
    {
      var valueTypesComparison = (x.ValueType - y.ValueType);
      if (x.Value == null) {
        if (y.Value == null)
          return valueTypesComparison * DefaultDirectionMultiplier;
        if (x.ValueType.IsInfinity()) {
          if (y.Value.Count == 1)
            return valueTypesComparison * DefaultDirectionMultiplier;
          return (int) x.ValueType * DefaultDirectionMultiplier;
        }
        return -valueTypesComparison * DefaultDirectionMultiplier;
      }
      if (y.Value == null)
        return valueTypesComparison * DefaultDirectionMultiplier;

      var countDiff = x.Value.Count - y.Value.Count;
      var valuesComparison = BaseComparer.Compare(x.Value, y.Value);

      if (valuesComparison != 0) {
        var itemIndex = Math.Abs(valuesComparison);
        if (itemIndex==x.Value.Count + 1)
          if (x.ValueType!=EntireValueType.Exact)
            return (int) x.ValueType * DefaultDirectionMultiplier * (int) ComparisonRules[itemIndex - 2].Value.Direction;
        if (itemIndex==y.Value.Count + 1)
          if (y.ValueType!=EntireValueType.Exact)
            return -(int) y.ValueType * DefaultDirectionMultiplier * (int) ComparisonRules[itemIndex - 2].Value.Direction;
        if (itemIndex == x.Value.Count) {
          var xIsInfinity = x.ValueType.IsInfinity();
          if (countDiff == 0) {
            var yIsInfinity = y.ValueType.IsInfinity();
            if (xIsInfinity || yIsInfinity)
              return valueTypesComparison * DefaultDirectionMultiplier * (int)ComparisonRules[x.Value.Count - 1].Value.Direction;
          }
          if (xIsInfinity)
            return (int)x.ValueType * DefaultDirectionMultiplier * (int)ComparisonRules[x.Value.Count - 1].Value.Direction;
        }
        if (itemIndex == y.Value.Count) {
          var yIsInfinity = y.ValueType.IsInfinity();
          if (yIsInfinity)
            return -(int)y.ValueType * DefaultDirectionMultiplier * (int)ComparisonRules[x.Value.Count - 1].Value.Direction;
        }
        return valuesComparison;
      }

      if (countDiff == 0)
        return valueTypesComparison * DefaultDirectionMultiplier * (int)ComparisonRules[x.Value.Count - 1].Value.Direction;
      if (countDiff < 0)
        return (int)x.ValueType * DefaultDirectionMultiplier * (int)ComparisonRules[x.Value.Count - 1].Value.Direction;
      return (int)y.ValueType * DefaultDirectionMultiplier * (int)ComparisonRules[y.Value.Count - 1].Value.Direction;
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


    // Constructor

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
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
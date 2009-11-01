// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.05

using System;
using Xtensive.Core;
using Xtensive.Core.Arithmetic;
using Xtensive.Core.Comparison;

namespace Xtensive.Indexing.Composite
{
  [Serializable]
  internal sealed class SegmentBoundEntireComparer<T>: WrappingComparer<SegmentBoundEntire<T>, T>,
    IComparer<SegmentBoundEntire<T>, SegmentBound<T>>,
    ISystemComparer<SegmentBoundEntire<T>>
  {
    [NonSerialized]
    private AdvancedComparer<IEntire<T>> entireComparer;

    [NonSerialized]
    private Func<IEntire<T>, T, int> asymmetricCompare;

    /// <inheritdoc/>
    protected override IAdvancedComparer<SegmentBoundEntire<T>> CreateNew(ComparisonRules rules)
    {
      return new SegmentBoundEntireComparer<T>(Provider, rules);
    }

    #region IComparer<SegmentBoundEntire<T>,SegmentBound<T>> Members

    public int Compare(SegmentBoundEntire<T> x, SegmentBound<T> y)
    {
      int result = asymmetricCompare(x.Entire, y.Value);
      if (result != 0)
        return result;

      return x.SegmentNumber.CompareTo(y.SegmentNumber);
    }

    #endregion

    #region ISystemComparer<SegmentBoundEntire<T>> Members

    public override int Compare(SegmentBoundEntire<T> x, SegmentBoundEntire<T> y)
    {
      int result = entireComparer.Compare(x.Entire, y.Entire);
      if (result != 0)
        return result;

      return x.SegmentNumber.CompareTo(y.SegmentNumber);
    }

    public override bool Equals(SegmentBoundEntire<T> x, SegmentBoundEntire<T> y)
    {
      return Compare(x, y) == 0;
    }

    public override int GetHashCode(SegmentBoundEntire<T> obj)
    {
      return entireComparer.GetHashCode(obj.Entire) ^ obj.SegmentNumber.GetHashCode();
    }

    public override Func<SegmentBoundEntire<T>, TSecond, int> GetAsymmetric<TSecond>()
    {
      return ((IComparer<SegmentBoundEntire<T>, TSecond>)(object)this).Compare;
    }

    #endregion

    private void Initialize()
    {
      entireComparer = Provider.GetComparer<IEntire<T>>().ApplyRules(ComparisonRules);
      asymmetricCompare = entireComparer.GetAsymmetric<T>();
    }


    // Constructors

    public SegmentBoundEntireComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo<T> baseValueRangeInfo = BaseComparer.ValueRangeInfo;
      SegmentBoundEntire<T> min = new SegmentBoundEntire<T>(Entire<T>.MinValue, 0);
      SegmentBoundEntire<T> max = new SegmentBoundEntire<T>(Entire<T>.MaxValue, Int32.MaxValue);
      ValueRangeInfo = new ValueRangeInfo<SegmentBoundEntire<T>>(
        true, min,
        true, max,
        baseValueRangeInfo.HasDeltaValue,
        baseValueRangeInfo.HasDeltaValue ? 
          new SegmentBoundEntire<T>(Entire<T>.Create(baseValueRangeInfo.DeltaValue), 0) : 
          new SegmentBoundEntire<T>(Entire<T>.Create(default(T)), 0));
      Initialize();
    }

    /// <inheritdoc/>
    public override void OnDeserialization(object sender)
    {
      base.OnDeserialization(sender);
      Initialize();
    }

  }
}
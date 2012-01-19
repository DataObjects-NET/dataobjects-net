// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2008.03.06

using System;
using Xtensive.Conversion;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Comparison
{
  /// <summary>
  /// Casting comparer - wraps some other comparer for type <typeparamref name="TTarget"/>
  /// </summary>
  /// <typeparam name="TSource">The type to compare.</typeparam>
  /// <typeparam name="TTarget">The base type of <typeparamref name="TSource"/> to provide a comparer for.</typeparam>
  [Serializable]
  public sealed class CastingComparer<TSource, TTarget>: AdvancedComparerBase<TTarget>
  {
    private static readonly Converter<TTarget, TSource> toSource = AdvancedConverterStruct<TTarget, TSource>.Default.Convert;
    private static readonly Converter<TSource, TTarget> toTarget = AdvancedConverterStruct<TSource, TTarget>.Default.Convert;
    private readonly AdvancedComparer<TSource> sourceComparer;

    internal class AsymmetricCompareHandler<TSecond>:
      IComparer<TTarget, TSecond>
    {
      private readonly Func<TSource, TSecond, int> baseCompare;

      public int Compare(TTarget x, TSecond y)
      {
        return baseCompare(toSource(x), y);
      }

      public AsymmetricCompareHandler(Func<TSource, TSecond, int> baseCompare)
      {
        this.baseCompare = baseCompare;
      }
    }

    /// <inheritdoc/>
    protected override IAdvancedComparer<TTarget> CreateNew(ComparisonRules rules)
    {
      return new CastingComparer<TSource, TTarget>(sourceComparer.ApplyRules(rules));
    }

    /// <inheritdoc/>
    public override int Compare(TTarget x, TTarget y)
    {
      return sourceComparer.Compare(toSource(x), toSource(y));
    }

    /// <inheritdoc/>
    public override bool Equals(TTarget x, TTarget y)
    {
      return sourceComparer.Equals(toSource(x), toSource(y));
    }

    /// <inheritdoc/>
    public override int GetHashCode(TTarget obj)
    {
      return sourceComparer.GetHashCode(toSource(obj));
    }

    /// <inheritdoc/>
    public override TTarget GetNearestValue(TTarget value, Direction direction)
    {
      return toTarget(sourceComparer.GetNearestValue(toSource(value), direction));
    }

    /// <inheritdoc/>
    public override Func<TTarget, TSecond, int> GetAsymmetric<TSecond>()
    {
      Func<TSource, TSecond, int> asymmetricCompare = sourceComparer.GetAsymmetric<TSecond>();
      if (asymmetricCompare == null)
        throw new NotSupportedException();
      AsymmetricCompareHandler<TSecond> h = new AsymmetricCompareHandler<TSecond>(asymmetricCompare);
      return h.Compare;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public CastingComparer(AdvancedComparer<TSource> sourceComparer)
      : base(sourceComparer.Provider, sourceComparer.ComparisonRules)
    {
      this.sourceComparer = sourceComparer;
      ValueRangeInfo<TSource> vi = sourceComparer.ValueRangeInfo;
      ValueRangeInfo =
        new ValueRangeInfo<TTarget>(
          vi.HasMinValue,
          vi.HasMinValue ? toTarget(vi.MinValue) : default(TTarget),
          vi.HasMaxValue,
          vi.HasMaxValue ? toTarget(vi.MaxValue) : default(TTarget),
          vi.HasDeltaValue,
          vi.HasDeltaValue ? toTarget(vi.DeltaValue) : default(TTarget));
    }
  }
}
// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: 
// Created:    2008.03.06

using System.Runtime.Serialization;
using Xtensive.Conversion;
using Xtensive.Core;
using Xtensive.Reflection;


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
    private class AsymmetricCompareHandler<TSecond> :
      IComparer<TTarget, TSecond>
    {
      private readonly Func<TSource, TSecond, int> baseCompare;

      public int Compare(TTarget x, TSecond y) => baseCompare(ToSource(x), y);

      public AsymmetricCompareHandler(Func<TSource, TSecond, int> baseCompare)
      {
        this.baseCompare = baseCompare;
      }
    }

    private static readonly Converter<TTarget, TSource> ToSource = AdvancedConverterStruct<TTarget, TSource>.Default.Convert;
    private static readonly Converter<TSource, TTarget> ToTarget = AdvancedConverterStruct<TSource, TTarget>.Default.Convert;

    private readonly AdvancedComparer<TSource> sourceComparer;

    /// <inheritdoc/>
    protected override IAdvancedComparer<TTarget> CreateNew(ComparisonRules rules)
      => new CastingComparer<TSource, TTarget>(sourceComparer.ApplyRules(rules));

    /// <inheritdoc/>
    public override int Compare(TTarget x, TTarget y)
      => sourceComparer.Compare(ToSource(x), ToSource(y));

    /// <inheritdoc/>
    public override bool Equals(TTarget x, TTarget y) => sourceComparer.Equals(ToSource(x), ToSource(y));

    /// <inheritdoc/>
    public override int GetHashCode(TTarget obj) => sourceComparer.GetHashCode(ToSource(obj));

    /// <inheritdoc/>
    public override TTarget GetNearestValue(TTarget value, Direction direction) => ToTarget(sourceComparer.GetNearestValue(ToSource(value), direction));

    /// <inheritdoc/>
    public override Func<TTarget, TSecond, int> GetAsymmetric<TSecond>()
    {
      var asymmetricCompare = sourceComparer.GetAsymmetric<TSecond>();
      if (asymmetricCompare == null) {
        throw new NotSupportedException();
      }

      var handler = new AsymmetricCompareHandler<TSecond>(asymmetricCompare);
      return handler.Compare;
    }


    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    public CastingComparer(AdvancedComparer<TSource> sourceComparer)
      : base(sourceComparer.Provider, sourceComparer.ComparisonRules)
    {
      this.sourceComparer = sourceComparer;
      var vi = sourceComparer.ValueRangeInfo;
      ValueRangeInfo =
        new ValueRangeInfo<TTarget>(
          vi.HasMinValue,
          vi.HasMinValue ? ToTarget(vi.MinValue) : default(TTarget),
          vi.HasMaxValue,
          vi.HasMaxValue ? ToTarget(vi.MaxValue) : default(TTarget),
          vi.HasDeltaValue,
          vi.HasDeltaValue ? ToTarget(vi.DeltaValue) : default(TTarget));
    }

    public CastingComparer(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
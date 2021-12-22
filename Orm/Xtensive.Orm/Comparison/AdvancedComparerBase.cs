// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2008.01.23

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security;
using Xtensive.Arithmetic;
using Xtensive.Collections;
using Xtensive.Core;


namespace Xtensive.Comparison
{
  /// <summary>
  /// Base class for <see cref="IAdvancedComparer{T}"/> implementations.
  /// </summary>
  /// <typeparam name="T">The type to compare.</typeparam>
  [Serializable]
  public abstract class AdvancedComparerBase<T>: IAdvancedComparer<T>,
    ISerializable,
    IDeserializationCallback
  {
    private static Arithmetic<T> cachedArithmetic;
    [NonSerialized]
    private ConcurrentDictionary<(ComparisonRules, AdvancedComparerBase<T>), AdvancedComparer<T>> cachedComparers = 
      new ConcurrentDictionary<(ComparisonRules, AdvancedComparerBase<T>), AdvancedComparer<T>>();

    private IComparerProvider provider;
    private ValueRangeInfo<T> valueRangeInfo;

    /// <summary>
    /// Gets comparison rules used by this comparer.
    /// </summary>
    protected readonly ComparisonRules ComparisonRules;

    /// <summary>
    /// Multiplier for default direction in <see cref="ComparisonRules"/>
    /// (either <see langword="1"/> or <see langword="-1"/>).
    /// </summary>
    protected readonly int DefaultDirectionMultiplier;

    /// <inheritdoc/>
    public IComparerProvider Provider
    {
      [DebuggerStepThrough]
      get => provider;
    }

    /// <inheritdoc/>
    ComparisonRules IAdvancedComparerBase.ComparisonRules
    {
      [DebuggerStepThrough]
      get => ComparisonRules;
    }

    /// <inheritdoc/>
    public ValueRangeInfo<T> ValueRangeInfo
    {
      get => valueRangeInfo;
      protected set {
        valueRangeInfo = ComparisonRules.Value.Direction != Direction.Negative
          ? value
          : value.Invert();
      }
    }

    /// <inheritdoc/>
    public AdvancedComparer<T> ApplyRules(ComparisonRules rules)
    {
      return cachedComparers.GetOrAdd((rules, this),
        key => {
          var (_rules, _this) = key;
          return new AdvancedComparer<T>(_this.CreateNew(_rules));
        });
    }

    /// <inheritdoc/>
    public virtual Func<T, TSecond, int> GetAsymmetric<TSecond>() => throw new NotSupportedException();

    int IComparer.Compare(object x, object y) => Compare((T) x, (T) y);

    /// <inheritdoc/>
    public abstract int Compare(T x, T y);

    /// <inheritdoc/>
    public abstract bool Equals(T x, T y);

    /// <inheritdoc/>
    public abstract int GetHashCode(T obj);

    /// <inheritdoc/>
    public virtual T GetNearestValue(T value, Direction direction)
    {
      if (!valueRangeInfo.HasDeltaValue) {
        throw new NotSupportedException();
      }
      var arithmetic = GetArithmetic();
      if (arithmetic == null) {
        throw new NotSupportedException();
      }

      return direction == ComparisonRules.Value.Direction
        ? valueRangeInfo.HasMaxValue && Equals(value, valueRangeInfo.MaxValue)
          ? value
          : arithmetic.Add(value, valueRangeInfo.DeltaValue)
        : valueRangeInfo.HasMinValue && Equals(value, valueRangeInfo.MinValue)
          ? value
          : arithmetic.Subtract(value, valueRangeInfo.DeltaValue);
    }

    /// <summary>
    /// Gets default <see cref="IArithmetic{T}"/> for type <typeparamref name="T"/>.
    /// </summary>
    /// <returns>Default arithmetic.</returns>
    protected static Arithmetic<T> GetArithmetic()
    {
      if (cachedArithmetic == null) {
        cachedArithmetic = Arithmetic<T>.Default;
      }
      return cachedArithmetic;
    }

    /// <summary>
    /// Wraps this instance with the <see cref="CastingComparer{T,TTarget}"/>.
    /// </summary>
    /// <typeparam name="TTarget">The type to provide the comparer for (by wrapping this comparer).</typeparam>
    public AdvancedComparer<TTarget> Cast<TTarget>()
    {
      return typeof(TTarget) == typeof(T)
        ? new AdvancedComparer<TTarget>(this as IAdvancedComparer<TTarget>)
        : new AdvancedComparer<TTarget>(new CastingComparer<T, TTarget>(new AdvancedComparer<T>(this)));
    }

    /// <summary>
    /// Creates new comparer of the same type, but using different comparison rules.
    /// </summary>
    /// <param name="rules">Comparison rules for the new comparer (relatively to this one).</param>
    /// <returns>New comparer of the same type, but using different comparison rules.</returns>
    protected abstract IAdvancedComparer<T> CreateNew(ComparisonRules rules);


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="provider">Comparer provider this comparer is bound to.</param>
    /// <param name="comparisonRules">Comparison rules.</param>
    public AdvancedComparerBase(IComparerProvider provider, ComparisonRules comparisonRules)
    {
      ArgumentValidator.EnsureArgumentNotNull(provider, "provider");
      valueRangeInfo = new ValueRangeInfo<T>(
        false, default(T),
        false, default(T),
        false, default(T));
      this.provider = provider;
      ComparisonRules = comparisonRules;
      DefaultDirectionMultiplier = comparisonRules.Value.Direction == Direction.Negative ? -1 : 1;
    }

    public AdvancedComparerBase(SerializationInfo info, StreamingContext context)
    {
      provider = (IComparerProvider) info.GetValue(nameof(provider), typeof(IComparerProvider));
      valueRangeInfo = (ValueRangeInfo<T>) info.GetValue(nameof(valueRangeInfo), typeof(ValueRangeInfo<T>));
      ComparisonRules = (ComparisonRules) info.GetValue(nameof(ComparisonRules), typeof(ComparisonRules));
      DefaultDirectionMultiplier = info.GetInt32(nameof(DefaultDirectionMultiplier));
    }

    /// <summary>
    /// Performs post-deserialization actions.
    /// </summary>
    /// <param name="sender"></param>
    public virtual void OnDeserialization(object sender)
    {
      if (provider == null) {
        provider = ComparerProvider.Default;
      }
      else if (provider.GetType() == typeof(ComparerProvider)) {
        provider = ComparerProvider.Default;
      }
      else if (provider is SystemComparerProvider) {
        provider = ComparerProvider.System;
      }
      cachedComparers = new ConcurrentDictionary<(ComparisonRules, AdvancedComparerBase<T>), AdvancedComparer<T>>();
    }

    [SecurityCritical]
    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue(nameof(provider), provider, provider.GetType());
      info.AddValue(nameof(valueRangeInfo), valueRangeInfo, valueRangeInfo.GetType());
      info.AddValue(nameof(ComparisonRules), ComparisonRules, ComparisonRules.GetType());
      info.AddValue(nameof(DefaultDirectionMultiplier), DefaultDirectionMultiplier);
    }
  }
}

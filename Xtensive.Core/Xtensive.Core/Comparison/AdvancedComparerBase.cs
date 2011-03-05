// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.23

using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xtensive.Arithmetic;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Threading;

namespace Xtensive.Comparison
{
  /// <summary>
  /// Base class for <see cref="IAdvancedComparer{T}"/> implementations.
  /// </summary>
  /// <typeparam name="T">The type to compare.</typeparam>
  [Serializable]
  public abstract class AdvancedComparerBase<T>: IAdvancedComparer<T>, 
    IDeserializationCallback
  {
    private static Arithmetic<T> cachedArithmetic;
    [NonSerialized]
    private ThreadSafeDictionary<ComparisonRules, AdvancedComparer<T>> cachedComparers = 
      ThreadSafeDictionary<ComparisonRules, AdvancedComparer<T>>.Create(new object());
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
      get { return provider; }
    }

    /// <inheritdoc/>
    ComparisonRules IAdvancedComparerBase.ComparisonRules 
    {
      [DebuggerStepThrough]
      get { return ComparisonRules; }
    }

    /// <inheritdoc/>
    public ValueRangeInfo<T> ValueRangeInfo
    {
      get { return valueRangeInfo; }
      protected set {
        if (ComparisonRules.Value.Direction!=Direction.Negative)
          valueRangeInfo = value;
        else
          valueRangeInfo = value.Invert();
      }
    }

    /// <inheritdoc/>
    public AdvancedComparer<T> ApplyRules(ComparisonRules rules)
    {
      return cachedComparers.GetValue(rules, 
        (_rules, _this) => new AdvancedComparer<T>(_this.CreateNew(_rules)), 
        this);
    }

    /// <inheritdoc/>
    public virtual Func<T, TSecond, int> GetAsymmetric<TSecond>()
    {
      throw new NotSupportedException();
    }

    int IComparer.Compare(object x, object y)
    {
      return Compare((T) x, (T) y);
    }

    /// <inheritdoc/>
    public abstract int Compare(T x, T y);

    /// <inheritdoc/>
    public abstract bool Equals(T x, T y);

    /// <inheritdoc/>
    public abstract int GetHashCode(T obj);

    /// <inheritdoc/>
    public virtual T GetNearestValue(T value, Direction direction)
    {
      if (!valueRangeInfo.HasDeltaValue)
        throw new NotSupportedException();
      Arithmetic<T> arithmetic = GetArithmetic();
      if (arithmetic==null)
        throw new NotSupportedException();

      if (direction==ComparisonRules.Value.Direction) {
        if (valueRangeInfo.HasMaxValue && Equals(value, valueRangeInfo.MaxValue))
          return value;
        else
          return arithmetic.Add(value, valueRangeInfo.DeltaValue);
      }
      else {
        if (valueRangeInfo.HasMinValue && Equals(value, valueRangeInfo.MinValue))
          return value;
        else
          return arithmetic.Subtract(value, valueRangeInfo.DeltaValue);
      }
    }

    /// <summary>
    /// Gets default <see cref="IArithmetic{T}"/> for type <typeparamref name="T"/>.
    /// </summary>
    /// <returns>Default arithmetic.</returns>
    protected static Arithmetic<T> GetArithmetic()
    {
      if (cachedArithmetic==null)
        cachedArithmetic = Arithmetic<T>.Default;
      return cachedArithmetic;
    }

    /// <summary>
    /// Wraps this instance with the <see cref="CastingComparer{T,TTarget}"/>.
    /// </summary>
    /// <typeparam name="TTarget">The type to provide the comparer for (by wrapping this comparer).</typeparam>
    public AdvancedComparer<TTarget> Cast<TTarget>()
    {
      if (typeof(TTarget)==typeof(T))
        return new AdvancedComparer<TTarget>(this as IAdvancedComparer<TTarget>);
      else
        return new AdvancedComparer<TTarget>(new CastingComparer<T, TTarget>(new AdvancedComparer<T>(this)));
    }

    /// <summary>
    /// Creates new comparer of the same type, but using different comparison rules.
    /// </summary>
    /// <param name="rules">Comparison rules for the new comparer (relatively to this one).</param>
    /// <returns>New comparer of the same type, but using different comparison rules.</returns>
    protected abstract IAdvancedComparer<T> CreateNew(ComparisonRules rules);


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
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
      DefaultDirectionMultiplier = comparisonRules.Value.Direction==Direction.Negative ? -1 : 1;
    }

    /// <see cref="SerializableDocTemplate.OnDeserialization" copy="true" />
    public virtual void OnDeserialization(object sender)
    {
      if (provider==null) 
        provider = ComparerProvider.Default;
      else if (provider.GetType()==typeof(ComparerProvider))
        provider = ComparerProvider.Default;
      else if (provider is SystemComparerProvider)
        provider = ComparerProvider.System;
      cachedComparers = ThreadSafeDictionary<ComparisonRules, AdvancedComparer<T>>.Create(new object());
    }
  }
}

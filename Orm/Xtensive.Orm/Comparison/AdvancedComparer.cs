// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.02.08

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security;
using Xtensive.Core;
using Xtensive.Reflection;

namespace Xtensive.Comparison
{
  /// <summary>
  /// Provides delegates allowing to call comparison methods faster.
  /// </summary>
  /// <typeparam name="T">The type of <see cref="IAdvancedComparer{T}"/> generic argument.</typeparam>
  [Serializable]
  public sealed class AdvancedComparer<T>: MethodCacheBase<IAdvancedComparer<T>>
  {
    private static Lazy<AdvancedComparer<T>> systemCached =
      new Lazy<AdvancedComparer<T>>(() => ComparerProvider.System.GetComparer<T>());

    private static Lazy<AdvancedComparer<T>> defaultCached =
      new Lazy<AdvancedComparer<T>>(() => ComparerProvider.Default.GetComparer<T>());

    /// <summary>
    /// Gets default advanced comparer for type <typeparamref name="T"/>
    /// (uses <see cref="ComparerProvider.Default"/> <see cref="ComparerProvider"/>).
    /// </summary>
    public static AdvancedComparer<T> Default {
      [DebuggerStepThrough]
      get {
        return defaultCached.Value;
      }
    }

    /// <summary>
    /// Gets system comparers exposed as <see cref="AdvancedComparer{T}"/> for type <typeparamref name="T"/>.
    /// </summary>
    public static AdvancedComparer<T> System {
      get {
        return systemCached.Value;
      }
    }

    /// <summary>
    /// Gets the underlying comparer implementation.
    /// </summary>
    public readonly IComparer<T> ComparerImplementation;

    /// <summary>
    /// Gets the underlying equality comparer implementation.
    /// </summary>
    public readonly IEqualityComparer<T> EqualityComparerImplementation;

    /// <summary>
    /// Gets the provider underlying comparer is associated with.
    /// </summary>
    public readonly IComparerProvider Provider;

    /// <summary>
    /// Gets comparison rules used by the underlying comparer.
    /// </summary>
    public readonly ComparisonRules ComparisonRules;

    #region IAdvancedComparer<T> method delegates

    /// <summary>
    /// Gets <see cref="IComparer{T}.Compare"/> method delegate.
    /// </summary>
    public readonly Func<T, T, int> Compare;

    /// <summary>
    /// Gets <see cref="IEqualityComparer{T}.Equals(T,T)"/> method delegate.
    /// </summary>
    public readonly new Predicate<T, T> Equals;

    /// <summary>
    /// Gets <see cref="IEqualityComparer{T}.GetHashCode(T)"/> method delegate.
    /// </summary>
    public readonly new Func<T, int> GetHashCode;

    /// <summary>
    /// Gets <see cref="INearestValueProvider{T}.GetNearestValue"/> method delegate.
    /// </summary>
    public readonly Func<T, Direction, T> GetNearestValue;

    /// <summary>
    /// Gets <see cref="IHasRangeInfo{T}.ValueRangeInfo"/> value used by the underlying comparer.
    /// </summary>
    public readonly ValueRangeInfo<T> ValueRangeInfo;

    /// <summary>
    /// Gets <see cref="IAdvancedComparer{T}.ApplyRules"/> method delegate.
    /// </summary>
    public readonly Func<ComparisonRules, AdvancedComparer<T>> ApplyRules;

    /// <summary>
    /// A shortcut to <see cref="IAdvancedComparer{T}.GetAsymmetric{TSecond}"/> method
    /// of <see cref="MethodCacheBase{TImplementation}.Implementation"/>.
    /// </summary>
    public Func<T, TSecond, int> GetAsymmetric<TSecond>()
    {
      return Implementation.GetAsymmetric<TSecond>();
    }

    /// <summary>
    /// Wraps this instance with the <see cref="CastingComparer{T,TTarget}"/>.
    /// </summary>
    /// <typeparam name="TTarget">The type to provide the comparer for (by wrapping this comparer).</typeparam>
    public AdvancedComparer<TTarget> Cast<TTarget>() 
    {
      if (typeof(TTarget)==typeof(T))
        return this as AdvancedComparer<TTarget>;
      else
        return Implementation.Cast<TTarget>();
    }

    #endregion


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="implementation">Implementation to provide the delegates for.</param>
    public AdvancedComparer(IAdvancedComparer<T> implementation)
      : this(implementation, implementation, implementation)
    {
      ArgumentValidator.EnsureArgumentNotNull(implementation, "implementation");
    }

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="comparer">Comparer to provide the delegates for.</param>
    /// <param name="equalityComparer">Equality comparer to provide the delegates for.</param>
    public AdvancedComparer(IComparer<T> comparer, IEqualityComparer<T> equalityComparer)
      : this(null, comparer, equalityComparer)
    {
    }

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="comparer">Comparer to provide the delegates for.</param>
    public AdvancedComparer(IComparer<T> comparer)
      : this(null, comparer, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="equalityComparer">Equality comparer to provide the delegates for.</param>
    public AdvancedComparer(IEqualityComparer<T> equalityComparer)
      : this(null, null, equalityComparer)
    {
    }

    private AdvancedComparer(IAdvancedComparer<T> a, IComparer<T> c, IEqualityComparer<T> e)
      : base(a)
    {
      ComparerImplementation = c;
      EqualityComparerImplementation = e;
      // Below code is the same between both primary constructors
      if (Implementation!=null) {
        Provider = Implementation.Provider;
        ComparisonRules = Implementation.ComparisonRules;
        ValueTypeComparerBase<T> vtc = Implementation as ValueTypeComparerBase<T>;
        SystemComparer<T> sc = Implementation as SystemComparer<T>;
        // Trying to get faster delegates
        if (sc!=null) {
          if (ComparisonRules.Value.Direction==Direction.Positive)
            Compare = SystemComparerStruct<T>.Instance.Compare;
          Equals = SystemComparerStruct<T>.Instance.Equals;
          GetHashCode = SystemComparerStruct<T>.Instance.GetHashCode;
        }
        else if (vtc!=null) {
          if (ComparisonRules.Value.Direction==Direction.Positive && vtc.UsesDefaultCompare)
            Compare = SystemComparerStruct<T>.Instance.Compare;
          if (vtc.UsesDefaultEquals)
            Equals = SystemComparerStruct<T>.Instance.Equals;
          if (vtc.UsesDefaultGetHashCode)
            GetHashCode = SystemComparerStruct<T>.Instance.GetHashCode;
        }
        // Setting interface comparers, if unassigned
        if (Compare==null)
          Compare = ComparerImplementation.Compare;
        if (Equals==null)
          Equals = EqualityComparerImplementation.Equals;
        if (GetHashCode==null)
          GetHashCode = EqualityComparerImplementation.GetHashCode;
        GetNearestValue = Implementation.GetNearestValue;
        ApplyRules = Implementation.ApplyRules;
        ValueRangeInfo = Implementation.ValueRangeInfo;
      }
      else {
        ComparisonRules = ComparisonRules.Positive;
        if (ComparerImplementation!=null)
          Compare = ComparerImplementation.Compare;
        if (EqualityComparerImplementation!=null) {
          Equals = EqualityComparerImplementation.Equals;
          GetHashCode = EqualityComparerImplementation.GetHashCode;
        }
      }
    }

    /// <summary>
    /// Deserializes the instance of this class.
    /// </summary>
    /// <param name="info">Serialization info.</param>
    /// <param name="context">Streaming context.</param>
    private AdvancedComparer(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      ComparerImplementation = (IComparer<T>)info.GetValue("ComparerImplementation", WellKnownTypes.Object);
      EqualityComparerImplementation = (IEqualityComparer<T>)info.GetValue("EqualityComparerImplementation", WellKnownTypes.Object);
      // Below code is the same between both primary constructors
      if (Implementation!=null) {
        Provider = Implementation.Provider;
        ComparisonRules = Implementation.ComparisonRules;
        ValueTypeComparerBase<T> vtc = Implementation as ValueTypeComparerBase<T>;
        SystemComparer<T> sc = Implementation as SystemComparer<T>;
        // Trying to get faster delegates
        if (sc!=null) {
          if (ComparisonRules.Value.Direction==Direction.Positive)
            Compare = SystemComparerStruct<T>.Instance.Compare;
          Equals = SystemComparerStruct<T>.Instance.Equals;
          GetHashCode = SystemComparerStruct<T>.Instance.GetHashCode;
        }
        else if (vtc!=null) {
          if (ComparisonRules.Value.Direction==Direction.Positive && vtc.UsesDefaultCompare)
            Compare = SystemComparerStruct<T>.Instance.Compare;
          if (vtc.UsesDefaultEquals)
            Equals = SystemComparerStruct<T>.Instance.Equals;
          if (vtc.UsesDefaultGetHashCode)
            GetHashCode = SystemComparerStruct<T>.Instance.GetHashCode;
        }
        // Setting interface comparers, if unassigned
        if (Compare==null)
          Compare = ComparerImplementation.Compare;
        if (Equals==null)
          Equals = EqualityComparerImplementation.Equals;
        if (GetHashCode==null)
          GetHashCode = EqualityComparerImplementation.GetHashCode;
        GetNearestValue = Implementation.GetNearestValue;
        ApplyRules = Implementation.ApplyRules;
        ValueRangeInfo = Implementation.ValueRangeInfo;
      }
      else {
        ComparisonRules = ComparisonRules.Positive;
        if (ComparerImplementation!=null)
          Compare = ComparerImplementation.Compare;
        if (EqualityComparerImplementation!=null) {
          Equals = EqualityComparerImplementation.Equals;
          GetHashCode = EqualityComparerImplementation.GetHashCode;
        }
      }
    }

    /// <inheritdoc/>
    [SecurityCritical]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData(info, context);
      info.AddValue("ComparerImplementation", ComparerImplementation);
      info.AddValue("EqualityComparerImplementation", EqualityComparerImplementation);
    }
  }
}
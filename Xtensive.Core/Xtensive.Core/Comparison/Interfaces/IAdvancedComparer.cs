// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.22

using System;
using System.Collections.Generic;

namespace Xtensive.Comparison
{
  /// <summary>
  /// Interface for any comparer supported by <see cref="ComparerProvider"/>.
  /// </summary>
  /// <typeparam name="T">The type of values this comparer can compare.</typeparam>
  public interface IAdvancedComparer<T>: IAdvancedComparerBase,
    IComparer<T>, 
    IEqualityComparer<T>,
    IHasRangeInfo<T>,
    INearestValueProvider<T>
  {
    /// <summary>
    /// Creates a new instance of <see cref="IAdvancedComparer{T}"/> 
    /// with specified comparison rules applied.
    /// </summary>
    /// <param name="rules">Rules to apply (relatively to <see cref="ComparisonRules"/> of this comparer).</param>
    /// <returns>New instance of <see cref="IAdvancedComparer{T}"/>.</returns>
    AdvancedComparer<T> ApplyRules(ComparisonRules rules);

    /// <summary>
    /// Gets the instance of <see cref="IComparer{TX,TY}"/> (asymmetric comparer)
    /// for <typeparamref name="T"/>-<typeparamref name="TSecond"/> pair, if supported.
    /// </summary>
    /// <typeparam name="TSecond">Type of the second argument to compare.</typeparam>
    /// <exception cref="InvalidOperationException">Complex comparer could not be 
    /// created for <typeparamref name="T"/>-<typeparamref name="TSecond"/> pair.</exception>
    /// <returns><see cref="IComparer{TX,TY}"/> asymmetric comparer.</returns>
    Func<T, TSecond, int> GetAsymmetric<TSecond>();

    /// <summary>
    /// Wraps this instance with the <see cref="CastingComparer{T,TTarget}"/>.
    /// </summary>
    /// <typeparam name="TTarget">The type to provide the comparer for (by wrapping this comparer).</typeparam>
    AdvancedComparer<TTarget> Cast<TTarget>();
  }
}
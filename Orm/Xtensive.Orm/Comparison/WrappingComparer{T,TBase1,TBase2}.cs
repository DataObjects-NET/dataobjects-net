// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.01.21

using System;
using Xtensive.Core;


namespace Xtensive.Comparison
{
  /// <summary>
  /// Base class for any wrapping <see cref="IAdvancedComparer{T}"/>s.
  /// </summary>
  /// <typeparam name="T">The type to compare.</typeparam>
  /// <typeparam name="TBase1">First base (wrapped) type.</typeparam>
  /// <typeparam name="TBase2">Second base (wrapped) type.</typeparam>
  public abstract class WrappingComparer<T, TBase1, TBase2>: AdvancedComparerBase<T>
  {
    /// <summary>
    /// Comparer delegates for <typeparamref name="TBase1"/> type.
    /// </summary>
    protected readonly AdvancedComparerStruct<TBase1> BaseComparer1;

    /// <summary>
    /// Comparer delegates for <typeparamref name="TBase2"/> type.
    /// </summary>
    protected readonly AdvancedComparerStruct<TBase2> BaseComparer2;

    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="provider">Comparer provider this comparer is bound to.</param>
    /// <param name="comparisonRules">Comparison rules.</param>
    public WrappingComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ArgumentValidator.EnsureArgumentNotNull(provider, "provider");
      BaseComparer1 = provider.GetComparer<TBase1>().ApplyRules(comparisonRules[0]);
      BaseComparer2 = provider.GetComparer<TBase2>().ApplyRules(comparisonRules[1]);
    }
  }
}
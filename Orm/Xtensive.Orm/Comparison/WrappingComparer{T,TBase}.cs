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
  /// <typeparam name="TBase">Base (wrapped) type.</typeparam>
  public abstract class WrappingComparer<T, TBase> : AdvancedComparerBase<T>
  {
    /// <summary>
    /// Comparer delegates for <typeparamref name="TBase"/> type.
    /// </summary>
    protected readonly AdvancedComparerStruct<TBase> BaseComparer;


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="provider">Comparer provider this comparer is bound to.</param>
    /// <param name="comparisonRules">Comparison rules.</param>
    public WrappingComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ArgumentNullException.ThrowIfNull(provider);
      BaseComparer = provider.GetComparer<TBase>().ApplyRules(comparisonRules);
    }
  }
}
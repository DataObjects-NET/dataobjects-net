// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.21

using System;
using Xtensive.Core;

using Xtensive.Internals.DocTemplates;

namespace Xtensive.Comparison
{
  /// <summary>
  /// Base class for any wrapping <see cref="IAdvancedComparer{T}"/>s.
  /// </summary>
  /// <typeparam name="T">The type to compare.</typeparam>
  /// <typeparam name="TBase">Base (wrapped) type.</typeparam>
  [Serializable]
  public abstract class WrappingComparer<T, TBase> : AdvancedComparerBase<T>
  {
    /// <summary>
    /// Comparer delegates for <typeparamref name="TBase"/> type.
    /// </summary>
    protected AdvancedComparerStruct<TBase> BaseComparer;


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="provider">Comparer provider this comparer is bound to.</param>
    /// <param name="comparisonRules">Comparison rules.</param>
    public WrappingComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ArgumentValidator.EnsureArgumentNotNull(provider, "provider");
      BaseComparer = provider.GetComparer<TBase>().ApplyRules(comparisonRules);
    }
  }
}
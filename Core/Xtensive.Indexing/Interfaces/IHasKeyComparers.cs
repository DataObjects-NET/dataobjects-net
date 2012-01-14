// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.22

using System;
using Xtensive.Comparison;

namespace Xtensive.Indexing
{
  /// <summary>
  /// An object having 3 key comparers contract.
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  public interface IHasKeyComparers<TKey>
  {
    /// <summary>
    /// Gets key comparer.
    /// </summary>
    AdvancedComparer<TKey> KeyComparer { get; }

    /// <summary>
    /// Gets the <see cref="Entire{T}"/> comparer for <typeparamref name="TKey"/> type.
    /// </summary>
    AdvancedComparer<Entire<TKey>> EntireKeyComparer { get; }

    /// <summary>
    /// Gets the delegate used to compare 
    /// <see cref="Entire{T}"/> for <typeparamref name="TKey"/> type and <typeparamref name="TKey"/> type.
    /// </summary>
    Func<Entire<TKey>, TKey, int> AsymmetricKeyCompare { get; }
  }
}
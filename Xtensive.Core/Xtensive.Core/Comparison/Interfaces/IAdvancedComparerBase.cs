// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.22

using System.Collections;

namespace Xtensive.Comparison
{
  /// <summary>
  /// Very base interface for any comparer supported by <see cref="IComparerProvider"/>.
  /// </summary>
  public interface IAdvancedComparerBase : IComparer
  {
    /// <summary>
    /// Gets the provider this comparer is associated with.
    /// </summary>
    IComparerProvider Provider { get; }

    /// <summary>
    /// Gets comparison rules used by this comparer.
    /// </summary>
    ComparisonRules ComparisonRules { get; }
  }
}

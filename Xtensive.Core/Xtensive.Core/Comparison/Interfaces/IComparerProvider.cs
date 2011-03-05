// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.01.14

using System.Collections.Generic;

namespace Xtensive.Comparison
{
  /// <summary>
  /// Provides <see cref="IAdvancedComparer{T}"/> comparers.
  /// </summary>
  public interface IComparerProvider
  {
    /// <summary>
    /// Gets comparer for the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type to get the comparer for.</typeparam>
    /// <returns>Comparer for <typeparamref name="T"/> type.</returns>
    AdvancedComparer<T> GetComparer<T>();
  }
}
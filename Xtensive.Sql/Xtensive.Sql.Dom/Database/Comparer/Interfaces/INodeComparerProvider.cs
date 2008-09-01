// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.18

using System;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// SQL comparer provider.
  /// </summary>
  public interface INodeComparerProvider
  {
    /// <summary>
    /// Gets <see cref="INodeComparer{T}"/> for the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type to get the comparer for.</typeparam>
    /// <returns><see cref="INodeComparer{T}"/> for the specified type <typeparamref name="T"/>.</returns>
    NodeComparer<T> GetNodeComparer<T>();

    /// <summary>
    /// Gets <see cref="INodeComparerBase"/> for the specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Value to get the comparer for.</param>
    /// <returns><see cref="INodeComparerBase"/> for the specified <paramref name="value"/>.</returns>
    INodeComparerBase GetNodeComparerByInstance(object value);

    /// <summary>
    /// Gets <see cref="INodeComparerBase"/> for the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type to get the comparer for.</param>
    /// <returns><see cref="INodeComparerBase"/> for the specified <paramref name="type"/>.</returns>
    INodeComparerBase GetNodeComparerByType(Type type);
  }
}
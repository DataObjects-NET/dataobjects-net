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
  public interface ISqlComparerProvider
  {
    /// <summary>
    /// Gets <see cref="ISqlComparer{T}"/> for the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type to get the comparer for.</typeparam>
    /// <returns><see cref="ISqlComparer{T}"/> for the specified type <typeparamref name="T"/>.</returns>
    SqlComparer<T> GetSqlComparer<T>();

    /// <summary>
    /// Gets <see cref="ISqlComparerBase"/> for the specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Value to get the comparer for.</param>
    /// <returns><see cref="ISqlComparerBase"/> for the specified <paramref name="value"/>.</returns>
    ISqlComparerBase GetSqlComparerByInstance(object value);

    /// <summary>
    /// Gets <see cref="ISqlComparerBase"/> for the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type to get the comparer for.</param>
    /// <returns><see cref="ISqlComparerBase"/> for the specified <paramref name="type"/>.</returns>
    ISqlComparerBase GetSqlComparerByType(Type type);
  }
}
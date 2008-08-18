// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.15

using System.Collections.Generic;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// <see cref="SchemaNode"/> comparer.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public interface ISqlComparer<T>
    where T : SchemaNode
  {
    /// <summary>
    /// Compares two instances of <see cref="SchemaNode"/>.
    /// </summary>
    /// <param name="originalNode">Original node.</param>
    /// <param name="newNode">New node.</param>
    /// <param name="hints">Hints for comparers.</param>
    /// <returns><see cref="CompareResult{T}"/> with result of compare.</returns>
    CompareResult<T> Compare(T originalNode, T newNode, IEnumerable<CompareHint> hints);
  }
}
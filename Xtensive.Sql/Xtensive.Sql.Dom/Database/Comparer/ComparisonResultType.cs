// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.15

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// Type of comparison result.
  /// </summary>
  public enum ComparisonResultType
  {
    /// <summary>
    /// Both compared nodes are equal.
    /// </summary>
    Unchanged,

    /// <summary>
    /// New node was added to scheme.
    /// </summary>
    Added,

    /// <summary>
    /// Original node was removed from scheme.
    /// </summary>
    Removed,

    /// <summary>
    /// Original node was modified and become new node.
    /// </summary>
    Modified,
  }
}
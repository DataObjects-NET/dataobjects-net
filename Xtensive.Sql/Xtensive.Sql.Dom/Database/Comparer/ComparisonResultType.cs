// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.15

using System;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// Type of comparison result.
  /// </summary>
  [Flags]
  public enum ComparisonResultType
  {
    /// <summary>
    /// Both compared nodes are equal.
    /// </summary>
    Unchanged = 0x00,

    /// <summary>
    /// New node is added to scheme.
    /// </summary>
    Added = 0x01,

    /// <summary>
    /// Original node is removed from scheme.
    /// </summary>
    Removed = 0x02,

    /// <summary>
    /// Original node is modified.
    /// </summary>
    Modified = 0x04,

    /// <summary>
    /// All comparison results.
    /// </summary>
    All = Added + Removed + Modified,
  }
}
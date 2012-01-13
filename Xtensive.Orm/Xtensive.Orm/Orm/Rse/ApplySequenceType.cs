// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.06.23

namespace Xtensive.Orm.Rse
{
  /// <summary>
  /// Apply operation type.
  /// </summary>
  public enum ApplySequenceType
  {
    /// <summary>
    /// <see cref="All"/>.
    /// </summary>
    Default = All,
    /// <summary>
    /// All rows.
    /// </summary>
    All = 0,
    /// <summary>
    /// Expects at least one row.
    /// </summary>
    First,
    /// <summary>
    /// Expects single row.
    /// </summary>
    Single,
    /// <summary>
    /// Applies only first row if it exists; otherwise applies default row.
    /// </summary>
    FirstOrDefault,
    /// <summary>
    /// Applies single row if it exists; otherwise applies default row.
    /// </summary>
    SingleOrDefault
  }
}
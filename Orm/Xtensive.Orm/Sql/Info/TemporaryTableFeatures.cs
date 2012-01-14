// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Defines a list of possible temporary table features.
  /// </summary>
  [Flags]
  public enum TemporaryTableFeatures
  {
    /// <summary>
    /// Indicates that RDBMS does not support temporary tables.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// Indicates that RDBMS supports globally visible temporary tables.
    /// </summary>
    Global = 0x1,

    /// <summary>
    /// Indicates that RDBMS supports temporary tables
    /// which are visible only in a local context.
    /// </summary>
    Local = 0x2,

    /// <summary>
    /// Indicates that RDBMS allows to delete rows on commit.
    /// </summary>
    DeleteRowsOnCommit = 0x4,

    /// <summary>
    /// Indicates that RDBMS allows to preserve rows on commit.
    /// </summary>
    PreserveRowsOnCommit = 0x8,
  }
}

// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Defines a list of RDBMS dependent view features.
  /// </summary>
  [Flags]
  public enum ViewFeatures
  {
    /// <summary>
    /// Indicates that RDBMS does not support features from this list.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// Indicates that RDBMS allows to create local views
    /// those are visible only in current execution context.
    /// </summary>
    Local = 0x1,

    /// <summary>
    /// Indicates that RDBMS allows to create views based on
    /// some other views instead of tables.
    /// </summary>
    Cascaded = 0x2,
  }
}

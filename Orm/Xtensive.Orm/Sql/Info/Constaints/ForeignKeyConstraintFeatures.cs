// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.20

using System;
using System.Diagnostics;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Possible features for <see cref="ForeignKeyConstraintInfo"/>.
  /// </summary>
  [Flags]
  public enum ForeignKeyConstraintFeatures
  {
    /// <summary>
    /// Indicates that RDBMS does not support any additional features
    /// for its constraints.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// Indicates that RDBMS supports deferrable foreign key constraints.
    /// </summary>
    Deferrable = 0x1,
  }
}
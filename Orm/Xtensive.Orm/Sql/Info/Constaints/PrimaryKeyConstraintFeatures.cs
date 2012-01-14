// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.20

using System;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Possible features for <see cref="PrimaryKeyConstraintInfo"/>.
  /// </summary>
  [Flags]
  public enum PrimaryKeyConstraintFeatures
  {
    /// <summary>
    /// Indicates that RDBMS does not support any additional features
    /// for its constraints.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// Indicates that primary key constraints can be declared clustered.
    /// </summary>
    Clustered = 0x1,

    /// <summary>
    /// Indicates that primary key constraints can be applied to nullable columns.
    /// </summary>
    Nullable = 0x2,
  }
}
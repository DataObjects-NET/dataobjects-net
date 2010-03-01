// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// <para>Defines optional features for RDBMS constraints.</para>
  /// <para>An exact feature set depends on a certain RDBMS capabilities.</para>
  /// </summary>
  [Flags]
  public enum ConstraintFeatures
  {
    /// <summary>
    /// Indicates that RDBMS does not support any additional features
    /// for its constraints.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// Indicates that primary and unique key constraints can be
    /// declared clustered.
    /// </summary>
    Clustered = 0x1,

    /// <summary>
    /// Indicates that RDBMS supports deferrable constraints.
    /// </summary>
    Deferrable = 0x2,

    /// <summary>
    /// Indicates that primary and unique key constraints can be
    /// applied to nullable columns.
    /// </summary>
    Nullable = 0x4,
  }
}

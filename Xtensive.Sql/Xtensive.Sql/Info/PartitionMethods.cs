// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// <para>Defines a list of possible horisontal partitioning types.</para>
  /// </summary>
  [Flags]
  public enum PartitionMethods
  {
    /// <summary>
    /// Indicates that RDBMS does not support partitioning.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// Indicates that RDBMS supports list partitioning.
    /// </summary>
    List = 0x1,

    /// <summary>
    /// Indicates that RDBMS supports range partitioning.
    /// </summary>
    Range = 0x2,

    /// <summary>
    /// Indicates that RDBMS supports hash partitioning.
    /// </summary>
    Hash = 0x4,
  }
}

// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Enumeration of possible partitioning methods.
  /// </summary>
  [Serializable]
  public enum PartitionMethod
  {
    /// <summary>
    /// None.
    /// Value is <see langword="0x00"/>. 
    /// </summary>
    None = 0,

    /// <summary>
    /// Indicates that table rows should be assigned to partitions 
    /// based on column values falling within a given range.
    /// Value is <see langword="0x01"/>. 
    /// </summary>
    Range = 1,

    /// <summary>
    /// Indicates that table rows should be assigned to partitions 
    /// based on columns values matching one of a set of discrete values.
    /// Value is <see langword="0x02"/>. 
    /// </summary>
    List = 2,

    /// <summary>
    /// Indicates that table rows should be assigned to partitions 
    /// based on the value returned by a user-defined expression 
    /// that operates on column values in rows to be inserted into the table.
    /// Value is <see langword="0x04"/>. 
    /// </summary>
    Hash = 4,
  }
}

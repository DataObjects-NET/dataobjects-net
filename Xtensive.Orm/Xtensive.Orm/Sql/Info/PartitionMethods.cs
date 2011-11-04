// Copyright (C) 2003-2010 Xtensive LLC.
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
    /// List partitioning enables you to explicitly control how rows map to partitions
    /// by specifying a list of discrete values in the description for each partition.
    /// </summary>
    List = 0x1,

    /// <summary>
    /// Indicates that RDBMS supports range partitioning.
    /// Range partitioning maps data to partitions
    /// based on ranges of partition key values that you establish for each partition.
    /// </summary>
    Range = 0x2,

    /// <summary>
    /// Indicates that RDBMS supports hash partitioning.
    /// Hash partitioning maps data to partitions
    /// based on a hashing algorithm that evenly distributes rows among partitions,
    /// giving partitions approximately the same size.
    /// </summary>
    Hash = 0x4,

    /// <summary>
    /// Indicates that RDBMS supports interval partitioning.
    /// Interval partitioning is an extension of range partitioning
    /// which instructs the database to automatically create partitions
    /// of a specified interval when data inserted into the table exceeds all of the range partitions.
    /// </summary>
    Interval = 0x8,
  }
}

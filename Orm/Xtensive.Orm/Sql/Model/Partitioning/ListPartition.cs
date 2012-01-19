// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents a list partition.
  /// </summary>
  [Serializable]
  public class ListPartition : Partition
  {
    /// <summary>
    /// Gets or sets the values.
    /// </summary>
    /// <value>The values.</value>
    public string[] Values { get; set; }

    #region Constructors

    internal ListPartition(PartitionDescriptor partitionDescriptor, string filegroup, params string[] values) : base(partitionDescriptor, filegroup)
    {
      Values = values;
    }

    #endregion
  }
}
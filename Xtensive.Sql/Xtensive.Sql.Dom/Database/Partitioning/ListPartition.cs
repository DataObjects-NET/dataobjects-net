// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dom.Database
{
  /// <summary>
  /// Represents a list partition.
  /// </summary>
  [Serializable]
  public class ListPartition : Partition
  {
    private string[] values;

    /// <summary>
    /// Gets or sets the values.
    /// </summary>
    /// <value>The values.</value>
    public string[] Values
    {
      get { return values; }
      set { values = value; }
    }

    #region Constructors

    internal ListPartition(PartitionDescriptor partitionDescriptor, string filegroup, params string[] values) : base(partitionDescriptor, filegroup)
    {
      this.values = values;
    }

    #endregion
  }
}
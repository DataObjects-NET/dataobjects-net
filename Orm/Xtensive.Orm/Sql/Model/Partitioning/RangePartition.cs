// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents a range partition.
  /// </summary>
  [Serializable]
  public class RangePartition : Partition
  {
    private string boundary;

    /// <summary>
    /// Gets or sets the partition boundary.
    /// </summary>
    /// <value>The boundary.</value>
    public string Boundary
    {
      get { return boundary; }
      set
      {
        EnsureNotLocked();
        boundary = value;
      }
    }

    #region Constructors

    internal RangePartition(PartitionDescriptor partitionDescriptor, string filegroup, string boundary)
      : base(partitionDescriptor, filegroup)
    {
      this.boundary = boundary;
    }

    #endregion
  }
}
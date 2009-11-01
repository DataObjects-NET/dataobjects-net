// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;
using Xtensive.Core.Helpers;

namespace Xtensive.Sql.Dom.Database
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
        this.EnsureNotLocked();
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
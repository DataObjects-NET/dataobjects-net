// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;
using Xtensive.Helpers;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents a single partition object.
  /// </summary>
  [Serializable]
  public abstract class Partition : Node, IPairedNode<PartitionDescriptor>
  {
    private PartitionDescriptor partitionDescriptor;

    /// <summary>
    /// Gets or sets the tablespace name or a filegroup name (according to RDBMS)
    /// that contains this instance.
    /// </summary>
    /// <value>The tablespace or a filegroup name.</value>
    public string Filegroup
    {
      get { return Name; }
      set { Name = value; }
    }

    /// <summary>
    /// Gets or sets the partitionDescriptor.
    /// </summary>
    /// <value>The partitionDescriptor.</value>
    public PartitionDescriptor PartitionDescriptor
    {
      get { return partitionDescriptor; }
      set {
        this.EnsureNotLocked();
        if (partitionDescriptor == value)
          return;
        if (partitionDescriptor!=null)
          partitionDescriptor.Partitions.Remove(this);
        partitionDescriptor = null;
        if (value!=null && !value.Partitions.Contains(this))
          value.Partitions.Add(this);
      }
    }

    #region IPairedNode<PartitionDescriptor> Members

    /// <summary>
    /// Updates the paired property.
    /// </summary>
    /// <param name="property">The collection property name.</param>
    /// <param name="value">The collection owner.</param>
    void IPairedNode<PartitionDescriptor>.UpdatePairedProperty(string property, PartitionDescriptor value)
    {
      this.EnsureNotLocked();
      partitionDescriptor = value;
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Partition"/> class.
    /// </summary>
    /// <param name="partitionDescriptor">The partition descriptor.</param>
    /// <param name="filegroup">The filegroup.</param>
    protected Partition(PartitionDescriptor partitionDescriptor, string filegroup) : base(filegroup)
    {
      PartitionDescriptor = partitionDescriptor;
    }

    #endregion
  }
}
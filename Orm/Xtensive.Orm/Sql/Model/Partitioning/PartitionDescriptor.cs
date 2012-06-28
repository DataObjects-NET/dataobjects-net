// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents a set of information concerning object partitioning.
  /// </summary>
  [Serializable]
  public class PartitionDescriptor : Node
  {
    private IPartitionable owner;
    private TableColumn column;
    private int partitionAmount;
    private PairedNodeCollection<PartitionDescriptor, Partition> partitions;
    private PartitionMethod partitionMethod;
    private PartitionSchema partitionSchema;

    /// <summary>
    /// Creates the hash partition.
    /// </summary>
    /// <param name="filegroup">The filegroup.</param>
    /// <returns></returns>
    public HashPartition CreateHashPartition(string filegroup)
    {
      return new HashPartition(this, filegroup);
    }

    /// <summary>
    /// Creates the list partition.
    /// </summary>
    /// <param name="filegroup">The filegroup.</param>
    /// <param name="values">The values.</param>
    /// <returns></returns>
    public ListPartition CreateListPartition(string filegroup, params string[] values)
    {
      return new ListPartition(this, filegroup, values);
    }

    /// <summary>
    /// Creates the range partition.
    /// </summary>
    /// <param name="filegroup">The filegroup.</param>
    /// <param name="boundary">The boundary.</param>
    /// <returns></returns>
    public RangePartition CreateRangePartition(string filegroup, string boundary)
    {
     return new RangePartition(this, filegroup, boundary); 
    }

    /// <summary>
    /// Gets or sets the owner.
    /// </summary>
    /// <value>The owner.</value>
    public IPartitionable Owner
    {
      get { return owner; }
      set {
        this.EnsureNotLocked();
        IPartitionable old = owner;
        owner = value;
        if (old!=null && old.PartitionDescriptor==this)
          old.PartitionDescriptor = null;
        if (owner!=null && owner.PartitionDescriptor!=this)
          owner.PartitionDescriptor = this;
      }
    }

    /// <summary>
    /// Gets or sets the column against which an object will be partitioned.
    /// </summary>
    /// <value>The column.</value>
    public TableColumn Column
    {
      get { return column; }
      set
      {
        this.EnsureNotLocked();
        column = value;
      }
    }

    /// <summary>
    /// Gets or sets the partition amount.
    /// </summary>
    /// <value>The partition amount.</value>
    public int PartitionAmount
    {
      get { return partitionAmount; }
      set
      {
        this.EnsureNotLocked();
        partitionAmount = value;
      }
    }

    /// <summary>
    /// Gets the partitions.
    /// </summary>
    /// <value>The partitions.</value>
    public PairedNodeCollection<PartitionDescriptor, Partition> Partitions
    {
      get { return partitions; }
    }

    /// <summary>
    /// Gets or sets the partition method.
    /// </summary>
    /// <value>The partition method.</value>
    public PartitionMethod PartitionMethod
    {
      get { return partitionMethod; }
      set
      {
        this.EnsureNotLocked();
        partitionMethod = value;
      }
    }

    /// <summary>
    /// Gets or sets the partition schema.
    /// </summary>
    /// <value>The partition schema.</value>
    public PartitionSchema PartitionSchema
    {
      get { return partitionSchema; }
      set
      {
        this.EnsureNotLocked();
        partitionSchema = value;
      }
    }

    #region Constructors

    private PartitionDescriptor(IPartitionable owner, TableColumn column)
    {
      this.owner = owner;
      Column = column;
      partitions = new PairedNodeCollection<PartitionDescriptor, Partition>(this, "Partitions");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PartitionDescriptor"/> class.
    /// </summary>
    /// <param name="owner">The owner.</param>
    /// <param name="column">The column.</param>
    /// <param name="partitionSchema">The partition schema.</param>
    public PartitionDescriptor(IPartitionable owner, TableColumn column, PartitionSchema partitionSchema) : this(owner, column)
    {
      this.partitionSchema = partitionSchema;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PartitionDescriptor"/> class.
    /// </summary>
    /// <param name="owner">The owner.</param>
    /// <param name="column">The column.</param>
    /// <param name="partitionMethod">The partition method.</param>
    /// <param name="partitionAmount">The partition amount.</param>
    public PartitionDescriptor(IPartitionable owner, TableColumn column, PartitionMethod partitionMethod, int partitionAmount) : this(owner, column)
    {
      this.partitionAmount = partitionAmount;
      this.partitionMethod = partitionMethod;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PartitionDescriptor"/> class.
    /// </summary>
    /// <param name="owner">The owner.</param>
    /// <param name="column">The column.</param>
    /// <param name="partitionMethod">The partition method.</param>
    public PartitionDescriptor(IPartitionable owner, TableColumn column, PartitionMethod partitionMethod) : this(owner, column)
    {
      this.partitionMethod = partitionMethod;
    }

    #endregion
  }
}
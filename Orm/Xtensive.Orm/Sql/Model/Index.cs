// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents an index.
  /// </summary>
  [Serializable]
  public class Index : DataTableNode, IPartitionable
  {
    private PairedNodeCollection<Index, IndexColumn> columns;
    private NodeCollection<DataTableColumn> nonkeyColumns;
    private bool isUnique;
    private bool isBitmap;
    private bool isClustered;
    private byte? fillFactor;
    private string filegroup;
    private PartitionDescriptor partitionDescriptor;
    private SqlExpression where;

    /// <summary>
    /// Creates the index column.
    /// </summary>
    /// <param name="column">The column.</param>
    public IndexColumn CreateIndexColumn(DataTableColumn column)
    {
      return CreateIndexColumn(column, true);
    }

    /// <summary>
    /// Creates the index column.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <param name="ascending">The sort direction.</param>
    public IndexColumn CreateIndexColumn(DataTableColumn column, bool ascending)
    {
      ArgumentValidator.EnsureArgumentNotNull(column, "column");
      return new IndexColumn(this, column, ascending);
    }

    /// <summary>
    /// Creates the index column.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <returns><see cref="IndexColumn"/> instance.</returns>
    public IndexColumn CreateIndexColumn(SqlExpression expression)
    {
      return CreateIndexColumn(expression, true);
    }

    /// <summary>
    /// Creates the index column.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <param name="ascending">Indicates whether the sorting order of the newly created <see cref="IndexColumn"/>
    /// is ascending.</param>
    /// <returns><see cref="IndexColumn"/> instance.</returns>
    public IndexColumn CreateIndexColumn(SqlExpression expression, bool ascending)
    {
      if (expression.IsNullReference())
        throw new ArgumentNullException("expression");
      return new IndexColumn(this, expression, ascending);
    }

    /// <summary>
    /// Columns, this instance is based on.
    /// </summary>
    public PairedNodeCollection<Index, IndexColumn> Columns
    {
      get { return columns; }
    }

    /// <summary>
    /// Non key columns to be added to the the index.
    /// </summary>
    public NodeCollection<DataTableColumn> NonkeyColumns
    {
      get { return nonkeyColumns; }
    }

    /// <summary>
    /// Gets or sets the index filter expression.
    /// </summary>
    public SqlExpression Where
    {
      get { return where; }
      set
      {
        this.EnsureNotLocked();
        where = value;
      }
    }

    /// <summary>
    /// Gets or sets the value indicating that this index is unique index.
    /// Unique index is one in which no two rows are permitted to have the same index key value.
    /// </summary>
    public bool IsUnique
    {
      get { return isUnique; }
      set
      {
        this.EnsureNotLocked();
        isUnique = value;
      }
    }

    /// <summary>
    /// Gets or sets the value indicating that this index is full-text index.
    /// </summary>
    public virtual bool IsFullText
    {
      get { return false; }
    }

    /// <summary>
    /// Gets or sets the value indicating that this index is spatial index.
    /// </summary>
    public virtual bool IsSpatial
    {
      get { return false; }
    }

    /// <summary>
    /// Gets or sets the value indicating that this index is bitmap index.
    /// A bitmap index is a special kind of index that stores the bulk of its data as bitmaps 
    /// and answers most queries by performing bitwise logical operations on these bitmaps.
    /// </summary>
    public bool IsBitmap
    {
      get { return isBitmap; }
      set
      {
        this.EnsureNotLocked();
        isBitmap = value;
      }
    }

    /// <summary>
    /// Gets or sets the value indicating that this index is clustered index.
    /// Clustered index is the index in which the logical order of the key values 
    /// determines the physical order of the corresponding rows in a table. 
    /// The bottom, or leaf, level of the clustered index contains the actual data rows 
    /// of the table. A table or view is allowed one clustered index at a time.
    /// </summary>
    public bool IsClustered
    {
      get { return isClustered; }
      set
      {
        this.EnsureNotLocked();
        isClustered = value;
      }
    }

    /// <summary>
    /// Specifies a percentage that indicates how full the database server should make the leaf level 
    /// of each index page during index creation or rebuild. 
    /// </summary>
    public byte? FillFactor
    {
      get { return fillFactor; }
      set
      {
        this.EnsureNotLocked();
        fillFactor = value;
      }
    }

    /// <summary>
    /// Gets or sets the value indicating that specified index will be created on the specified tablespace or filegroup. 
    /// If no location is specified and the table or view is not partitioned, the index uses the same filegroup 
    /// as the underlying table or view.
    /// </summary>
    public string Filegroup
    {
      get { return filegroup; }
      set
      {
        this.EnsureNotLocked();
        filegroup = value;
      }
    }

    /// <summary>
    /// Gets or sets the partition descriptor.
    /// </summary>
    /// <value>The partition descriptor.</value>
    public PartitionDescriptor PartitionDescriptor
    {
      get { return partitionDescriptor; }
      set {
        this.EnsureNotLocked();
        PartitionDescriptor old = partitionDescriptor;
        partitionDescriptor = value;
        if (old!=null && old.Owner==this)
          old.Owner = null;
        if (partitionDescriptor!=null && partitionDescriptor.Owner!=this)
          partitionDescriptor.Owner = this;
      }
    }

    #region DataTableNode Members

    /// <summary>
    /// Changes the data table.
    /// </summary>
    /// <param name="value">The value.</param>
    protected override void ChangeDataTable(DataTable value)
    {
      if (DataTable!=null)
        DataTable.Indexes.Remove(this);
      if (value!=null)
        value.Indexes.Add(this);
    }

    #endregion

    #region ILockable Members

    /// <summary>
    /// Locks the instance and (possible) all dependent objects.
    /// </summary>
    /// <param name="recursive"><see langword="True"/> if all dependent objects should be locked too.</param>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      columns.Lock(recursive);
      nonkeyColumns.Lock(recursive);
      if (partitionDescriptor!=null)
        partitionDescriptor.Lock(recursive);
    }

    #endregion

    #region Constructors

    internal Index(DataTable dataTable, string name) : base(dataTable, name)
    {
      nonkeyColumns = new NodeCollection<DataTableColumn>();
      columns = new PairedNodeCollection<Index, IndexColumn>(this, "Columns");
    }

    #endregion
  }
}
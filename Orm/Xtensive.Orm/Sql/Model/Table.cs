// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents a table object.
  /// </summary>
  [Serializable]
  public class Table : DataTable,IConstrainable,IPartitionable
  {
    private readonly PairedNodeCollection<Table, TableColumn> columns;
    private readonly PairedNodeCollection<Table, TableConstraint> constraints;
    private PartitionDescriptor partitionDescriptor;
    private string filegroup;

    /// <summary>
    /// Creates the table column.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="dataType">Column datatype.</param>
    public TableColumn CreateColumn(string name, SqlValueType dataType)
    {
      return new TableColumn(this, name, dataType);
    }

    /// <summary>
    /// Creates the table column.
    /// </summary>
    /// <param name="name">The name.</param>
    public TableColumn CreateColumn(string name)
    {
      return new TableColumn(this, name, null);
    }

    /// <summary>
    /// Creates the check constraint.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="condition">The condition.</param>
    public CheckConstraint CreateCheckConstraint(string name, SqlExpression condition)
    {
      return new CheckConstraint(this, name, condition);
    }

    /// <summary>
    /// Creates the default constraint.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="column">The column.</param>
    /// <returns>Default constraint.</returns>
    public DefaultConstraint CreateDefaultConstraint(string name, TableColumn column)
    {
      return new DefaultConstraint(this, name, column);
    }

    /// <summary>
    /// Creates the primary key.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="columns">The columns.</param>
    public PrimaryKey CreatePrimaryKey(string name, params TableColumn[] columns)
    {
      return new PrimaryKey(this, name, columns);
    }

    /// <summary>
    /// Creates the unique constraint.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="columns">The columns.</param>
    public UniqueConstraint CreateUniqueConstraint(string name, params TableColumn[] columns)
    {
      return new UniqueConstraint(this, name, columns);
    }

    /// <summary>
    /// Creates the foreign key.
    /// </summary>
    /// <param name="name">The name.</param>
    public ForeignKey CreateForeignKey(string name)
    {
      return new ForeignKey(this, name);
    }

    /// <summary>
    /// Gets the columns.
    /// </summary>
    /// <value>The columns.</value>
    public PairedNodeCollection<Table, TableColumn> TableColumns
    {
      get { return columns; }
    }

    /// <summary>
    /// Gets the constraints.
    /// </summary>
    /// <value>The constraints.</value>
    public PairedNodeCollection<Table, TableConstraint> TableConstraints
    {
      get { return constraints; }
    }

    /// <summary>
    /// Gets or sets the partitioning descriptor.
    /// </summary>
    /// <value>The partitioning descriptor.</value>
    public PartitionDescriptor PartitionDescriptor
    {
      get { return partitionDescriptor; }
      set
      {
        this.EnsureNotLocked();
        PartitionDescriptor old = partitionDescriptor;
        partitionDescriptor = value;
        if (old != null && old.Owner == this)
          old.Owner = null;
        if (partitionDescriptor != null && partitionDescriptor.Owner != this)
          partitionDescriptor.Owner = this;
      }
    }

    /// <summary>
    /// Gets or sets the value indicating that specified table will be created on the specified tablespace or filegroup. 
    /// If no location is specified and the table or view is not partitioned, the table will bw located at the 
    /// default filegroup.
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

    #region DataTable Members

    /// <summary>
    /// Gets the columns.
    /// </summary>
    /// <value>The columns.</value>
    public override IList<DataTableColumn> Columns
    {
      get { return TableColumns.ToArray().Convert(i => (DataTableColumn) i); }
    }

    #endregion

    #region IConstrainable Members

    /// <summary>
    /// Gets the node constraints.
    /// </summary>
    /// <value>The constraints.</value>
    IList<Constraint> IConstrainable.Constraints
    {
      get { return constraints.ToArray().Convert(i => (Constraint)i); }
    }

    #endregion

    #region SchemaNode Members

    /// <summary>
    /// Changes the schema.
    /// </summary>
    /// <param name="value">The value.</param>
    protected override void ChangeSchema(Schema value)
    {
      if (Schema != null)
        Schema.Tables.Remove(this);
      if (value != null)
        value.Tables.Add(this);
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
      constraints.Lock(recursive);
      columns.Lock(recursive);
      if (partitionDescriptor != null)
        partitionDescriptor.Lock(recursive);
    }

    #endregion
    
    #region Constructors

    internal Table(Schema schema, string name)
      : base(schema, name)
    {
      columns =
        new PairedNodeCollection<Table, TableColumn>(this, "TableColumns");
      constraints =
        new PairedNodeCollection<Table, TableConstraint>(this, "TableConstraints");
    }

    #endregion
  }
}
// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;
using Xtensive.Helpers;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents a <see cref="Table"/> bound column.
  /// </summary>
  [Serializable]
  public class TableColumn : DataTableColumn, IPairedNode<Table>,ISequenceable
  {
    private SqlValueType dataType;
    private Domain domain;
    private SqlExpression defaultValue;
    private SequenceDescriptor sequenceDescriptor;
    private SqlExpression expression;
    private bool isPersisted;
    private Collation collation;
    private bool isNullable;

    /// <summary>
    /// Gets or sets the <see cref="SqlValueType"/>.
    /// </summary>
    /// <value>The datatype.</value>
    public SqlValueType DataType
    {
      get { return dataType; }
      set
      {
        this.EnsureNotLocked();
        dataType = value;
      }
    }

    /// <summary>
    /// Gets or sets the domain.
    /// </summary>
    /// <value>The domain.</value>
    public Domain Domain
    {
      get { return domain; }
      set
      {
        this.EnsureNotLocked();
        domain = value;
      }
    }

    /// <summary>
    /// Specifies the value provided for the column when a value is not explicitly supplied during an insert. 
    /// </summary>
    public SqlExpression DefaultValue
    {
      get { return defaultValue; }
      set
      {
        this.EnsureNotLocked();
        defaultValue = value;
      }
    }

    /// <summary>
    /// Gets or sets the sequence descriptor.
    /// </summary>
    /// <value>The sequence descriptor.</value>
    public SequenceDescriptor SequenceDescriptor
    {
      get { return sequenceDescriptor; }
      set {
        SequenceDescriptor old = sequenceDescriptor;
        sequenceDescriptor = value;
        if (old!=null && old.Owner==this)
          old.Owner = null;
        if (sequenceDescriptor!=null && sequenceDescriptor.Owner!=this)
          sequenceDescriptor.Owner = this;
      }
    }

    /// <summary>
    /// Gets or sets the computed expression that defines the value of a computed column.
    /// </summary>
    /// <value>The expression of a computed column.</value>
    public SqlExpression Expression
    {
      get { return expression; }
      set
      {
        this.EnsureNotLocked();
        expression = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether RDBMS will physically store the computed values in the table, 
    /// and update the values when any other columns on which the computed column depends are updated. .
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if this instance is persisted; otherwise, <see langword="false"/>.
    /// </value>
    public bool IsPersisted
    {
      get { return isPersisted; }
      set
      {
        this.EnsureNotLocked();
        isPersisted = value;
      }
    }

    /// <summary>
    /// Gets or sets the collation.
    /// </summary>
    /// <value>The collation.</value>
    public Collation Collation
    {
      get { return collation; }
      set
      {
        this.EnsureNotLocked();
        collation = value;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether null values are allowed in the column.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if this instance is nullable; otherwise, <see langword="false"/>.
    /// </value>
    public bool IsNullable
    {
      get { return isNullable; }
      set
      {
        this.EnsureNotLocked();
        isNullable = value;
      }
    }

    /// <summary>
    /// Gets or sets the <see cref="Table"/> this instance belongs to.
    /// </summary>
    /// <value>The table.</value>
    public Table Table
    {
      get { return (Table)DataTable; }
      set
      {
        this.EnsureNotLocked();
        DataTable = value;
      }
    }

   
    #region DataTableNode Members

    /// <summary>
    /// Changes the data table.
    /// </summary>
    /// <param name="value">The value.</param>
    protected override void ChangeDataTable(DataTable value)
    {
      Table oldTable = DataTable as Table;
      Table newTable = value as Table;
      if (oldTable!=null)
        oldTable.TableColumns.Remove(this);
      if (newTable!=null)
        newTable.TableColumns.Add(this);
    }

    #endregion

    #region IPairedNode<Table> Members

    /// <summary>
    /// Updates the paired property.
    /// </summary>
    /// <param name="property">The collection property name.</param>
    /// <param name="value">The collection owner.</param>
    public void UpdatePairedProperty(string property, Table value)
    {
      ((IPairedNode<DataTable>)this).UpdatePairedProperty(property, value);
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
      if (sequenceDescriptor!=null)
        sequenceDescriptor.Lock(recursive);
    }

    #endregion

    internal TableColumn(Table table, string name, SqlValueType dataType) : base(table, name)
    {
      this.dataType = dataType;
    }
  }
}

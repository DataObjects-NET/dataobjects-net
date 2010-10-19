// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents a foreign key constraint that provides referential integrity 
  /// for the data in the column or columns. <see cref="ForeignKey"/> constraints require that 
  /// each value in the column exists in the corresponding referenced column or columns 
  /// in the referenced table.
  /// </summary>
  [Serializable]
  public class ForeignKey : TableConstraint
  {
    private NodeCollection<TableColumn> columns = new NodeCollection<TableColumn>();
    private Table referencedTable;
    private NodeCollection<TableColumn> referencedColumns = new NodeCollection<TableColumn>();
    private SqlMatchType matchType = SqlMatchType.None;
    private ReferentialAction onUpdate = ReferentialAction.NoAction;
    private ReferentialAction onDelete = ReferentialAction.NoAction;

    /// <summary>
    /// Gets the referencing columns.
    /// </summary>
    /// <value>The referencing columns.</value>
    public NodeCollection<TableColumn> Columns
    {
      get { return columns; }
    }

    /// <summary>
    /// Gets the referenced columns.
    /// </summary>
    /// <value>The referenced columns.</value>
    public NodeCollection<TableColumn> ReferencedColumns
    {
      get { return referencedColumns; }
    }

    /// <summary>
    /// Gets or sets the referenced table.
    /// </summary>
    /// <value>The referenced table.</value>
    public Table ReferencedTable
    {
      get { return referencedTable; }
      set { referencedTable = value; }
    }

    /// <summary>
    /// Match type.
    /// </summary>
    /// <value></value>
    public SqlMatchType MatchType
    {
      get { return matchType; }
      set { matchType = value; }
    }

    /// <summary>
    /// Specifies what action happens to rows in the table
    /// when those rows have a referential relationship
    /// and the referenced row is updated in the parent table.
    /// </summary>
    /// <value></value>
    public ReferentialAction OnUpdate
    {
      get { return onUpdate; }
      set { onUpdate = value; }
    }

    /// <summary>
    /// Specifies what action happens to rows in the table,
    /// if those rows have a referential relationship and
    /// the referenced row is deleted from the parent table.
    /// </summary>
    /// <value></value>
    public ReferentialAction OnDelete
    {
      get { return onDelete; }
      set { onDelete = value; }
    }

    #region ILockable Members

    /// <summary>
    /// Locks the instance and (possible) all dependent objects.
    /// </summary>
    /// <param name="recursive"><see langword="True"/> if all dependent objects should be locked too.</param>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      columns.Lock(recursive);
      referencedColumns.Lock(recursive);
    }

    #endregion

    #region Constructors

    internal ForeignKey(Table table, string name) : base(table, name)
    {
    }

    #endregion
  }
}
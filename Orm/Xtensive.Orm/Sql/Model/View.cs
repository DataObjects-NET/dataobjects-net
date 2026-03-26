// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Collections;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents a View node.
  /// </summary>
  [Serializable]
  public class View : DataTable
  {
    private PairedNodeCollection<View, ViewColumn> columns;
    private CheckOptions checkOptions = CheckOptions.None;
    private SqlNative definition;

    /// <summary>
    /// Creates the view column.
    /// </summary>
    /// <param name="name">The name.</param>
    public ViewColumn CreateColumn(string name)
    {
      return new ViewColumn(this, name);
    }

    /// <summary>
    /// Gets the columns.
    /// </summary>
    /// <value>The columns.</value>
    public PairedNodeCollection<View, ViewColumn> ViewColumns
    {
      get { return columns; }
    }

    /// <summary>
    /// Specifies the level of checking to be done when inserting or updating data through a view.
    /// If the option is specified, every row that is inserted or updated through the view must
    /// conform to the definition of that view.
    /// </summary>
    public CheckOptions CheckOptions
    {
      get { return checkOptions; }
      set {
        EnsureNotLocked();
        checkOptions = value;
      }
    }

    /// <summary>
    /// The statement that defines the view. The statement can use more than one table
    /// and other views.
    /// </summary>
    public SqlNative Definition
    {
      get { return definition; }
      set
      {
        EnsureNotLocked();
        definition = value;
      }
    }

   
    #region SchemaNode Members

    /// <summary>
    /// Changes the schema.
    /// </summary>
    /// <param name="value">The value.</param>
    protected override void ChangeSchema(Schema value)
    {
      if (Schema!=null)
        Schema.Views.Remove(this);
      if (value!=null)
        value.Views.Add(this);
    }

    #endregion

    #region DataTable Members

    /// <summary>
    /// Gets the columns.
    /// </summary>
    /// <value>The columns.</value>
    public override IList<DataTableColumn> Columns {
      get {
        return ViewColumns.ToArray().Convert(i => (DataTableColumn) i);
      }
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
    }

    #endregion

    #region Constructors

    internal View(Schema schema, string name)
      : base(schema, name)
    {
      columns = new PairedNodeCollection<View, ViewColumn>(this, "ViewColumns");
    }

    internal View(Schema schema, string name, SqlNative definition)
      : this(schema, name, definition, CheckOptions.None)
    {
    }

    internal View(Schema schema, string name, SqlNative definition, CheckOptions checkOptions)
      : base(schema, name)
    {
      columns = new PairedNodeCollection<View, ViewColumn>(this, "ViewColumns");
      this.definition = definition;
      this.checkOptions = checkOptions;
    }

    #endregion
  }
}
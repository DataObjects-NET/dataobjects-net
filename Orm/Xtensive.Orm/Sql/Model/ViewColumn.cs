// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represent a <see cref="View"/> bound column.
  /// </summary>
  [Serializable]
  public class ViewColumn : DataTableColumn, IPairedNode<View>
  {
    /// <summary>
    /// Gets or sets the <see cref="View"/> this instance belongs to.
    /// </summary>
    /// <value>The view.</value>
    public View View
    {
      get { return (View) DataTable; }
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
      View oldView = DataTable as View;
      View newView = value as View;
      if (oldView!=null)
        oldView.ViewColumns.Remove(this);
      if (newView!=null)
        newView.ViewColumns.Add(this);
    }

    #endregion

    #region IPairedNode<View> Members

    /// <summary>
    /// Updates the paired property.
    /// </summary>
    /// <param name="property">The collection property name.</param>
    /// <param name="value">The collection owner.</param>
    public void UpdatePairedProperty(string property, View value)
    {
      ((IPairedNode<DataTable>)this).UpdatePairedProperty(property, value);
    }

    #endregion

    #region Constructors

    internal ViewColumn(View view, string name) : base(view, name)
    {
    }

    #endregion
  }
}
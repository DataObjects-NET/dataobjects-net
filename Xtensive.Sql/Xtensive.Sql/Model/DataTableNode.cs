// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;
using Xtensive.Helpers;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents a <see cref="DataTable"/> bound object.
  /// </summary>
  [Serializable]
  public abstract class DataTableNode : Node, IPairedNode<DataTable>
  {
    private DataTable dataTable;

    /// <summary>
    /// Gets or sets the <see cref="DataTable"/> this instance belongs to.
    /// </summary>
    /// <value>The dataTable this instance belongs to.</value>
    public DataTable DataTable
    {
      get { return dataTable; }
      protected set {
        this.EnsureNotLocked();
        if (dataTable != value) {
          ChangeDataTable(value);
        }
      }
    }

    /// <summary>
    /// Changes the data table.
    /// </summary>
    /// <param name="value">The value.</param>
    protected abstract void ChangeDataTable(DataTable value);

  
    #region IPairedNode<DataTable> Members

    /// <summary>
    /// Updates the paired property.
    /// </summary>
    /// <param name="property">The collection property name.</param>
    /// <param name="value">The collection owner.</param>
    void IPairedNode<DataTable>.UpdatePairedProperty(string property, DataTable value)
    {
      this.EnsureNotLocked();
      dataTable = value;
    }

    #endregion


    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="DataTableNode"/> class.
    /// </summary>
    /// <param name="dataTable">The dataTable.</param>
    /// <param name="name">The name.</param>
    protected DataTableNode(DataTable dataTable, string name) : base(name)
    {
      ArgumentValidator.EnsureArgumentNotNull(dataTable, "dataTable");
      DataTable = dataTable;
    }

    #endregion
  }
}
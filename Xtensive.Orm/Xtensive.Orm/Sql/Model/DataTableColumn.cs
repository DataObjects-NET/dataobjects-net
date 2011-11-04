// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represent a <see cref="DataTable"/> bound column.
  /// </summary>
  [Serializable]
  public abstract class DataTableColumn : DataTableNode
  {
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="DataTableColumn"/> class.
    /// </summary>
    /// <param name="dataTable">The data table.</param>
    /// <param name="name">The name.</param>
    protected DataTableColumn(DataTable dataTable, string name) : base(dataTable, name)
    {
    }

    #endregion
  }
}
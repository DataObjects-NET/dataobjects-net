// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Collections;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents unique table constraint.
  /// </summary>
  [Serializable]
  public class UniqueConstraint : TableConstraint
  {
    private NodeCollection<TableColumn> columns;
    private bool isClustered;

    public bool IsClustered {
      get { return isClustered; }
      set {
        EnsureNotLocked();
        isClustered = value;
      }
    }

    /// <summary>
    /// Gets the columns.
    /// </summary>
    /// <value>The columns.</value>
    public NodeCollection<TableColumn> Columns
    {
      get { return columns; }
    }

      /// <summary>
      /// Locks the instance and (possible) all dependent objects.
      /// </summary>
      /// <param name="recursive"><see langword="True"/> if all dependent objects should be locked too.</param>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      columns.Lock(recursive);
    }

    #region Constructors

    internal UniqueConstraint(Table table, string name, params TableColumn[] columns) : base(table, name, null, null, null)
    {
      this.columns = new NodeCollection<TableColumn>(columns.Length);
      for (int i = 0, count = columns.Length; i<count; i++)
        this.columns.Add(columns[i]);
    }

    #endregion
  }
}
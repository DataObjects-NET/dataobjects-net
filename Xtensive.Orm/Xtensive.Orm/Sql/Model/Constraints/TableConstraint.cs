// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents a base <see cref="Table"/> constraint object.
  /// </summary>
  [Serializable]
  public abstract class TableConstraint
    : Constraint<Table>
  {
    /// <summary>
    /// Gets or sets the <see cref="Table"/> this instance belongs to.
    /// </summary>
    /// <value>The table.</value>
    public Table Table
    {
      get { return Owner; }
      set { Owner = value; }
    }

    /// <summary>
    /// Changes the table.
    /// </summary>
    /// <param name="value">The value.</param>
    protected override void ChangeOwner(Table value)
    {
      if (Table != null)
        Table.TableConstraints.Remove(this);
      if (value != null)
        value.TableConstraints.Add(this);
    }

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="TableConstraint"/> class.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <param name="name">The name.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="isDeferrable">Is constraint deferrable.</param>
    /// <param name="isInitiallyDeferred">Is constraint initially deferred.</param>
    protected TableConstraint(Table table, string name, SqlExpression condition, bool? isDeferrable,
                              bool? isInitiallyDeferred)
      : base(name, condition, isDeferrable, isInitiallyDeferred)
    {
      Table = table;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TableConstraint"/> class.
    /// </summary>
    /// <param name="table">The table.</param>
    /// <param name="name">The name.</param>
    protected TableConstraint(Table table, string name)
      : this(table, name, null, null, null)
    {
    }

    #endregion

  }
}
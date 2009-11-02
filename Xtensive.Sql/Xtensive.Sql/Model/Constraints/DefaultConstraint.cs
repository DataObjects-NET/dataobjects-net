// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.06.07

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents <see cref="Table"/> default constraint.
  /// </summary>
  [Serializable]
  public sealed class DefaultConstraint : TableConstraint
  {
    /// <summary>
    /// Gets the column.
    /// </summary>
    public TableColumn Column { get; private set; }


    // Constructors

    internal DefaultConstraint(Table table, string name, TableColumn column)
      : base(table, name)
    {
      ArgumentValidator.EnsureArgumentNotNull(column, "column");
      Column = column;
    }
  }
}
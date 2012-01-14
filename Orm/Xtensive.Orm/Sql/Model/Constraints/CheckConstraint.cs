// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql.Model
{
  /// <summary>
  /// Represents <see cref="Table"/> check constraint.
  /// </summary>
  public class CheckConstraint : TableConstraint
  {
    // Constructors

    internal CheckConstraint(Table table, string name, SqlExpression condition)
      : base(table, name, condition, null, null)
    {
    }
  }
}
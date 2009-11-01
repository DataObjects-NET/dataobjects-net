// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Dom.Dml;

namespace Xtensive.Sql.Dom.Database
{
  /// <summary>
  /// Represents <see cref="Table"/> check constraint.
  /// </summary>
  [Serializable]
  public class CheckConstraint : TableConstraint
  {
    #region Constructors

    internal CheckConstraint(Table table, string name, SqlExpression condition) : base(table, name, condition, null, null)
    {
    }

    #endregion
  }
}
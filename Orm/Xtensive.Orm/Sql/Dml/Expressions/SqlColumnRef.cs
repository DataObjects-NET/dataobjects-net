// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Represents a reference to a SQL column.
  /// </summary>
  [Serializable]
  public class SqlColumnRef : SqlColumn
  {
    /// <summary>
    /// Gets the SQL column.
    /// </summary>
    public SqlColumn SqlColumn { get; private set; }

    public override void ReplaceWith(SqlExpression expression)
    {
      SqlColumn = ArgumentValidator.EnsureArgumentIs<SqlColumnRef>(expression).SqlColumn;
    }

    internal override object Clone(SqlNodeCloneContext context) =>
      context.NodeMapping.TryGetValue(this, out var clone)
        ? clone
        : context.NodeMapping[this] = new SqlColumnRef(
            SqlTable != null ? (SqlTable) SqlTable.Clone(context) : null,
            (SqlColumn) SqlColumn.Clone(context), Name);

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    // Constructors

    internal SqlColumnRef(SqlTable sqlTable, SqlColumn sqlColumn, string name)
      : base(sqlTable, name)
    {
      SqlColumn = sqlColumn;
    }

    internal SqlColumnRef(SqlColumn sqlColumn, string name)
      : base(null, name)
    {
      SqlColumn = sqlColumn;
    }

    internal SqlColumnRef(SqlColumn sqlColumn)
    {
      SqlColumn = sqlColumn;
    }
  }
}

// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dom.Dml
{
  /// <summary>
  /// Represents a reference to a Sql column.
  /// </summary>
  [Serializable]
  public class SqlColumnRef : SqlColumn
  {
    private SqlColumn sqlColumn;

    /// <summary>
    /// Gets the SQL column.
    /// </summary>
    /// <value>The SQL column.</value>
    public SqlColumn SqlColumn
    {
      get { return sqlColumn; }
    }

    public override void ReplaceWith(SqlExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentIs<SqlColumnRef>(expression, "expression");
      sqlColumn = ((SqlColumnRef)expression).SqlColumn;
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlColumnRef clone = new SqlColumnRef(SqlTable, sqlColumn, Name);
      context.NodeMapping[this] = clone;
      return clone;
    }

    // Constructor

    internal SqlColumnRef(SqlTable sqlTable, SqlColumn sqlColumn, string name) : base(sqlTable, name)
    {
      this.sqlColumn = sqlColumn;
    }

    internal SqlColumnRef(SqlColumn sqlColumn, string name) : base(name)
    {
      this.sqlColumn = sqlColumn;
    }

    internal SqlColumnRef(SqlColumn sqlColumn)
    {
      this.sqlColumn = sqlColumn;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}

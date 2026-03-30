// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

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

    internal override SqlColumnRef Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlColumnRef(
            t.SqlTable?.Clone(c),
            (SqlColumn) t.SqlColumn.Clone(c), t.Name));

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

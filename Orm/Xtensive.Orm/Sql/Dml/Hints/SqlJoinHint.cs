// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Represents a join option (hint) for a query optimizer.
  /// </summary>
  [Serializable]
  public class SqlJoinHint : SqlHint
  {
    /// <summary>
    /// Gets the join method.
    /// </summary>
    /// <value>The join method.</value>
    public SqlJoinMethod Method { get; private set; }

    /// <summary>
    /// Gets the table.
    /// </summary>
    public SqlTable Table { get; private set; }

    internal override SqlJoinHint Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlJoinHint(t.Method, (SqlTable) t.Table.Clone()));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    // Constructors

    internal SqlJoinHint(SqlJoinMethod method, SqlTable table)
    {
      Method = method;
      Table = table;
    }
  }
}

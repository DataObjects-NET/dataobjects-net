// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

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

    internal override object Clone(SqlNodeCloneContext context) =>
      context.NodeMapping.TryGetValue(this, out var clone)
        ? clone
        : context.NodeMapping[this] = new SqlJoinHint(Method, (SqlTable) Table.Clone());

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

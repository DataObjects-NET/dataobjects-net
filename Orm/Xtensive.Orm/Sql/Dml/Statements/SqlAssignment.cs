// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;

namespace Xtensive.Sql.Dml
{
  public class SqlAssignment : SqlStatement
  {

    /// <summary>
    /// Gets the left operand of the assign statement.
    /// </summary>
    public ISqlLValue Left { get; }

    /// <summary>
    /// Gets the right operand of the assign statement.
    /// </summary>
    public SqlExpression Right { get; }

    internal override SqlAssignment Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlAssignment((ISqlLValue)t.Left.Clone(), t.Right.Clone(c)));

    internal SqlAssignment(ISqlLValue left, SqlExpression right)
      : base(SqlNodeType.Assign)
    {
      Right = right;
      Left = left;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}

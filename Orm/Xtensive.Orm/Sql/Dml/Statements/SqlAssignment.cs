// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlAssignment : SqlStatement
  {
    private readonly ISqlLValue left;
    private readonly SqlExpression right;

    /// <summary>
    /// Gets the left operand of the assign statement.
    /// </summary>
    public ISqlLValue Left {
      get {
        return left;
      }
    }

    /// <summary>
    /// Gets the right operand of the assign statement.
    /// </summary>
    public SqlExpression Right {
      get {
        return right;
      }
    }

    internal override object Clone(SqlNodeCloneContext context) =>
      context.NodeMapping.TryGetValue(this, out var clone)
        ? clone
        : context.NodeMapping[this] = new SqlAssignment((ISqlLValue)left.Clone(), (SqlExpression)right.Clone(context));

    internal SqlAssignment(ISqlLValue left, SqlExpression right)
      : base(SqlNodeType.Assign)
    {
      this.right = right;
      this.left = left;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}

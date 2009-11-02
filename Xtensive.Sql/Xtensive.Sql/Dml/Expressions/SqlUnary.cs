// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Represents unary expression.
  /// </summary>
  [Serializable]
  public class SqlUnary : SqlExpression
  {
    private SqlExpression operand;

    /// <summary>
    /// Gets the operand of the unary operator.
    /// </summary>
    /// <value>The operand of the unary operator.</value>
    public SqlExpression Operand {
      get {
        return operand;
      }
    }

    public override void ReplaceWith(SqlExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentIs<SqlUnary>(expression, "expression");
      SqlUnary replacingExpression = expression as SqlUnary;
      NodeType = replacingExpression.NodeType;
      operand = replacingExpression.Operand;
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlUnary clone = new SqlUnary(NodeType, (SqlExpression)operand.Clone(context));
      context.NodeMapping[this] = clone;
      return clone;
    }

    internal SqlUnary(SqlNodeType nodeType, SqlExpression operand) : base(nodeType)
    {
      this.operand = operand;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}

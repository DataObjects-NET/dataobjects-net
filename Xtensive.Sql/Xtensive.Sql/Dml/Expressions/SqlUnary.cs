// Copyright (C) 2003-2010 Xtensive LLC.
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
    /// <summary>
    /// Gets the operand of the unary operator.
    /// </summary>
    /// <value>The operand of the unary operator.</value>
    public SqlExpression Operand { get; private set; }

    public override void ReplaceWith(SqlExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentIs<SqlUnary>(expression, "expression");
      var replacingExpression = (SqlUnary) expression;
      NodeType = replacingExpression.NodeType;
      Operand = replacingExpression.Operand;
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      var clone = new SqlUnary(NodeType, (SqlExpression) Operand.Clone(context));
      context.NodeMapping[this] = clone;
      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    // Constructors

    internal SqlUnary(SqlNodeType nodeType, SqlExpression operand)
      : base(nodeType)
    {
      Operand = operand;
    }
  }
}

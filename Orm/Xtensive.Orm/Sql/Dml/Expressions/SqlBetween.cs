// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Represents between expression.
  /// </summary>
  [Serializable]
  public class SqlBetween: SqlExpression
  {
    private SqlExpression left;
    private SqlExpression right;
    private SqlExpression expression;

    /// <summary>
    /// Gets the left boundary of the between predicate.
    /// </summary>
    /// <value>The left boundary of the between predicate.</value>
    public SqlExpression Left
    {
      get { return left; }
    }

    /// <summary>
    /// Gets the right boundary of the between predicate.
    /// </summary>
    /// <value>The right boundary of the between predicate.</value>
    public SqlExpression Right
    {
      get { return right; }
    }

    /// <summary>
    /// Gets the expression to compare.
    /// </summary>
    /// <value>The expression to compare.</value>
    public SqlExpression Expression
    {
      get { return expression; }
    }

    public override void ReplaceWith(SqlExpression expression)
    {
      SqlBetween replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlBetween>(expression);
      NodeType = replacingExpression.NodeType;
      left = replacingExpression.Left;
      right = replacingExpression.Right;
      expression = replacingExpression.Expression;
    }

    internal override object Clone(SqlNodeCloneContext context) =>
      context.NodeMapping.TryGetValue(this, out var clone)
        ? clone
        : context.NodeMapping[this] = new SqlBetween(NodeType, (SqlExpression)expression.Clone(context), (SqlExpression)left.Clone(context), (SqlExpression)right.Clone(context));

    internal SqlBetween(SqlNodeType nodeType, SqlExpression expression, SqlExpression left, SqlExpression right)
      : base(nodeType)
    {
      this.expression = expression;
      this.left = left;
      this.right = right;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}
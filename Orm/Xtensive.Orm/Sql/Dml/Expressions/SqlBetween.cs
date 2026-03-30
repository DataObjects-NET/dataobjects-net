// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

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
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlBetween>(expression);
      NodeType = replacingExpression.NodeType;
      left = replacingExpression.Left;
      right = replacingExpression.Right;
      expression = replacingExpression.Expression;
    }

    internal override SqlBetween Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlBetween(t.NodeType, t.expression.Clone(c), t.left.Clone(c), t.right.Clone(c)));

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
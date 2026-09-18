// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Represents LIKE predicat.
  /// </summary>
  public class SqlLike : SqlExpression
  {

    /// <summary>
    /// Gets the expression.
    /// </summary>
    /// <value>The expression.</value>
    public SqlExpression Expression { get; private set; }

    /// <summary>
    /// Gets the pattern expression.
    /// </summary>
    /// <value>The pattern.</value>
    public SqlExpression Pattern { get; private set; }

    /// <summary>
    /// Gets the escape character expression.
    /// </summary>
    /// <value>The escape.</value>
    public SqlExpression Escape { get; private set; }

    public bool Not { get; private set; } = false;

    public override void ReplaceWith(SqlExpression expression)
    {
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlLike>(expression);
      Expression = replacingExpression.Expression;
      Pattern = replacingExpression.Pattern;
      Escape = replacingExpression.Escape;
      Not = replacingExpression.Not;
    }

    internal override SqlLike Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlLike(t.Expression.Clone(c),
            t.Pattern.Clone(c),
            t.Escape?.Clone(c), t.Not));

    internal SqlLike(SqlExpression expression, SqlExpression pattern, SqlExpression escape, bool not) : base (SqlNodeType.Like)
    {
      Expression = expression;
      Pattern = pattern;
      Escape = escape;
      Not = not;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}

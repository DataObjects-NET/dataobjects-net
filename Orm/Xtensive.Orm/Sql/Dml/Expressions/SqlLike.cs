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
  [Serializable]
  public class SqlLike : SqlExpression
  {
    private SqlExpression expression;
    private SqlExpression pattern;
    private SqlExpression escape;
    private bool not = false;

    /// <summary>
    /// Gets the expression.
    /// </summary>
    /// <value>The expression.</value>
    public SqlExpression Expression {
      get {
        return expression;
      }
    }

    /// <summary>
    /// Gets the pattern expression.
    /// </summary>
    /// <value>The pattern.</value>
    public SqlExpression Pattern {
      get {
        return pattern;
      }
    }

    /// <summary>
    /// Gets the escape character expression.
    /// </summary>
    /// <value>The escape.</value>
    public SqlExpression Escape {
      get {
        return escape;
      }
    }

    public bool Not {
      get {
        return not;
      }
    }

    public override void ReplaceWith(SqlExpression expression)
    {
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlLike>(expression);
      this.expression = replacingExpression.expression;
      pattern = replacingExpression.Pattern;
      escape = replacingExpression.Escape;
      not = replacingExpression.Not;
    }

    internal override SqlLike Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlLike(t.expression.Clone(c),
            t.pattern.Clone(c),
            t.escape?.Clone(c), t.not));

    internal SqlLike(SqlExpression expression, SqlExpression pattern, SqlExpression escape, bool not) : base (SqlNodeType.Like)
    {
      this.expression = expression;
      this.pattern = pattern;
      this.escape = escape;
      this.not = not;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}

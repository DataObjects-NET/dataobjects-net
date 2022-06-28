// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

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
      SqlLike replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlLike>(expression);
      this.expression = replacingExpression.expression;
      pattern = replacingExpression.Pattern;
      escape = replacingExpression.Escape;
      not = replacingExpression.Not;
    }

    internal override object Clone(SqlNodeCloneContext context) =>
      context.NodeMapping.TryGetValue(this, out var clone)
        ? clone
        : context.NodeMapping[this] = new SqlLike((SqlExpression) expression.Clone(context),
            (SqlExpression) pattern.Clone(context),
            escape is null ? null : (SqlExpression) escape.Clone(context), not);

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

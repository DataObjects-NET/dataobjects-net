// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dom.Dml
{
  /// <summary>
  /// Represents Trim function call.
  /// </summary>
  [Serializable]
  public class SqlTrim : SqlExpression
  {
    private SqlExpression expression;
    private SqlExpression pattern;
    private SqlTrimType trimType;

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

    public SqlTrimType TrimType
    {
      get { return trimType; }
    }

    public override void ReplaceWith(SqlExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentIs<SqlTrim>(expression, "expression");
      SqlTrim replacingExpression = expression as SqlTrim;
      this.expression = replacingExpression.expression;
      pattern = replacingExpression.Pattern;
      trimType = replacingExpression.TrimType;
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];
      
      SqlTrim clone = new SqlTrim((SqlExpression)expression.Clone(context),
                                  pattern != null ?  (SqlExpression)pattern.Clone(context) : null,
                                  trimType);
      context.NodeMapping[this] = clone;
      return clone;
    }

    internal SqlTrim(SqlExpression expression, SqlExpression pattern, SqlTrimType trimType) : base (SqlNodeType.Trim)
    {
      this.expression = expression;
      this.pattern = pattern;
      this.trimType = trimType;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}

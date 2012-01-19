// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlUserColumn : SqlColumn
  {
    private SqlExpression expression;

    /// <summary>
    /// Gets the column expression.
    /// </summary>
    /// <value>The expression.</value>
    public SqlExpression Expression
    {
      get { return expression; }
    }

    public override void ReplaceWith(SqlExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentIs<SqlUserColumn>(expression, "expression");
      SqlUserColumn replacingExpression = expression as SqlUserColumn;
      this.expression = replacingExpression.Expression;
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlColumn clone;
      clone = new SqlUserColumn((SqlExpression)expression.Clone(context));

      context.NodeMapping[this] = clone;
      return clone;
    }

    // Constructor

    internal SqlUserColumn(SqlExpression expression)
    {
      this.expression = expression;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}

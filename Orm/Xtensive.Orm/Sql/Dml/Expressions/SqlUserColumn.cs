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
      SqlUserColumn replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlUserColumn>(expression);
      this.expression = replacingExpression.Expression;
    }

    internal override SqlUserColumn Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlUserColumn(t.expression.Clone(c)));

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

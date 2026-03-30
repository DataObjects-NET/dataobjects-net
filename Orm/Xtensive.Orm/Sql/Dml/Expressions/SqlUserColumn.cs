// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

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
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlUserColumn>(expression);
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

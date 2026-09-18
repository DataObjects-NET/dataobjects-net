// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  public class SqlUserColumn : SqlColumn
  {
    /// <summary>
    /// Gets the column expression.
    /// </summary>
    /// <value>The expression.</value>
    public SqlExpression Expression { get; private set; }

    public override void ReplaceWith(SqlExpression expression)
    {
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlUserColumn>(expression);
      Expression = replacingExpression.Expression;
    }

    internal override SqlUserColumn Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlUserColumn(t.Expression.Clone(c)));

    // Constructor

    internal SqlUserColumn(SqlExpression expression)
    {
      Expression = expression;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}

// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  public class SqlAggregate : SqlExpression
  {

    /// <summary>
    /// Gets a value indicating whether this <see cref="SqlAggregate"/> is distinct.
    /// </summary>
    /// <value><see langword="true"/> if distinct; otherwise, <see langword="false"/>.</value>
    public bool Distinct { get; private set; }

    /// <summary>
    /// Gets the expression.
    /// </summary>
    /// <value>The expression.</value>
    public SqlExpression Expression { get; private set; }

    public override void ReplaceWith(SqlExpression expression)
    {
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlAggregate>(expression);
      NodeType = replacingExpression.NodeType;
      Distinct = replacingExpression.Distinct;
      Expression = replacingExpression.Expression;
    }

    internal override SqlAggregate Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlAggregate(t.NodeType,
            t.Expression?.Clone(c), t.Distinct));

    internal SqlAggregate(SqlNodeType nodeType, SqlExpression expression, bool distinct) : base(nodeType)
    {
      Expression = expression;
      Distinct = distinct;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}

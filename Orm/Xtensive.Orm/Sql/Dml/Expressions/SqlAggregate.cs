// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlAggregate : SqlExpression
  {
    private bool distinct;
    private SqlExpression expression;

    /// <summary>
    /// Gets a value indicating whether this <see cref="SqlAggregate"/> is distinct.
    /// </summary>
    /// <value><see langword="true"/> if distinct; otherwise, <see langword="false"/>.</value>
    public bool Distinct 
    {
      get { return distinct; }
    }

    /// <summary>
    /// Gets the expression.
    /// </summary>
    /// <value>The expression.</value>
    public SqlExpression Expression
    {
      get { return expression; }
    }

    public override void ReplaceWith(SqlExpression expression)
    {
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlAggregate>(expression);
      NodeType = replacingExpression.NodeType;
      distinct = replacingExpression.Distinct;
      this.expression = replacingExpression.Expression;
    }

    internal override SqlAggregate Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlAggregate(t.NodeType,
            t.expression?.Clone(c), t.distinct));

    internal SqlAggregate(SqlNodeType nodeType, SqlExpression expression, bool distinct) : base(nodeType)
    {
      this.expression = expression;
      this.distinct = distinct;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}

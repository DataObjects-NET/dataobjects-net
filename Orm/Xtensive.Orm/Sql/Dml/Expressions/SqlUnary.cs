// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Represents unary expression.
  /// </summary>
  [Serializable]
  public class SqlUnary : SqlExpression
  {
    /// <summary>
    /// Gets the operand of the unary operator.
    /// </summary>
    /// <value>The operand of the unary operator.</value>
    public SqlExpression Operand { get; private set; }

    public override void ReplaceWith(SqlExpression expression)
    {
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlUnary>(expression);
      NodeType = replacingExpression.NodeType;
      Operand = replacingExpression.Operand;
    }

    internal override SqlUnary Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlUnary(t.NodeType, t.Operand.Clone(c)));

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    // Constructors

    internal SqlUnary(SqlNodeType nodeType, SqlExpression operand)
      : base(nodeType)
    {
      Operand = operand;
    }
  }
}

// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlCast : SqlExpression
  {
    public SqlExpression Operand { get; private set; }
    public SqlValueType Type { get; private set; }

    public override void ReplaceWith(SqlExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentIs<SqlCast>(expression, "expression");
      var replacingExpression = (SqlCast) expression;
      Operand = replacingExpression.Operand;
      Type = replacingExpression.Type;
    }

    internal override object Clone(SqlNodeCloneContext context) =>
      context.NodeMapping.TryGetValue(this, out var clone)
        ? clone
        : context.NodeMapping[this] = new SqlCast((SqlExpression) Operand.Clone(context), Type);

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlCast(SqlExpression operand, SqlValueType type)
      : base(SqlNodeType.Cast)
    {
      Operand = operand;
      Type = type;
    }
  }
}

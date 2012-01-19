// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

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

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      var clone = new SqlCast((SqlExpression) Operand.Clone(context), Type);
      context.NodeMapping[this] = clone;
      return clone;
    }
    
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

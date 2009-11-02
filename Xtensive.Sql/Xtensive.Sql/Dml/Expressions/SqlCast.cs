// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlCast : SqlExpression
  {
    private SqlExpression operand;
    private SqlValueType type;
    
    public SqlExpression Operand {
      get {
        return operand;
      }
    }

    public SqlValueType Type {
      get {
        return type;
      }
    }

    public override void ReplaceWith(SqlExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentIs<SqlCast>(expression, "expression");
      SqlCast replacingExpression = expression as SqlCast;
      operand = replacingExpression.Operand;
      type = replacingExpression.Type;
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlCast clone = new SqlCast((SqlExpression)operand.Clone(context), type);
      context.NodeMapping[this] = clone;
      return clone;
    }
    
    internal SqlCast(SqlExpression operand, SqlValueType type)
      : base(SqlNodeType.Cast)
    {
      this.operand = operand;
      this.type = type;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}

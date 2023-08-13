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
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlCast>(expression);
      Operand = replacingExpression.Operand;
      Type = replacingExpression.Type;
    }

    internal override SqlCast Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlCast(t.Operand.Clone(c), t.Type));
    
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

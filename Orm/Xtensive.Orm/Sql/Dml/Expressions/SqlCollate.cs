// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlCollate : SqlExpression
  {
    private Collation collation;
    private SqlExpression operand;

    /// <summary>
    /// Gets the collation.
    /// </summary>
    /// <value>The collation.</value>
    public Collation Collation {
      get {
        return collation;
      }
    }

    /// <summary>
    /// Gets the operand.
    /// </summary>
    /// <value>The operand.</value>
    public SqlExpression Operand {
      get {
        return operand;
      }
    }

    public override void ReplaceWith(SqlExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentIs<SqlCollate>(expression, "expression");
      SqlCollate replacingExpression = expression as SqlCollate;
      operand = replacingExpression.Operand;
      collation = replacingExpression.Collation;
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlCollate clone = new SqlCollate((SqlExpression)operand.Clone(context), collation);
      context.NodeMapping[this] = clone;
      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal SqlCollate(SqlExpression operand, Collation collation) : base(SqlNodeType.Collate)
    {
      this.operand = operand;
      this.collation = collation;
    }
  }
}

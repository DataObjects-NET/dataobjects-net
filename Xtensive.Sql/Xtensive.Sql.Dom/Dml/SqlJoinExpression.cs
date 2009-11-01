// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;

namespace Xtensive.Sql.Dom.Dml
{
  [Serializable]
  public class SqlJoinExpression : SqlNode
  {
    private readonly SqlJoinType joinType;
    private readonly SqlTable left;
    private readonly SqlTable right;
    private readonly SqlExpression expression;

    /// <summary>
    /// Gets the type of the join.
    /// </summary>
    /// <value>The type of the join.</value>
    public SqlJoinType JoinType
    {
      get { return joinType; }
    }

    /// <summary>
    /// Gets the left.
    /// </summary>
    /// <value>The left.</value>
    public SqlTable Left
    {
      get { return left; }
    }

    /// <summary>
    /// Gets the right.
    /// </summary>
    /// <value>The right.</value>
    public SqlTable Right
    {
      get { return right; }
    }

    /// <summary>
    /// Gets the expression.
    /// </summary>
    /// <value>The expression.</value>
    public SqlExpression Expression
    {
      get { return expression; }
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];
      
      SqlJoinExpression clone = new SqlJoinExpression(joinType,
                                  left==null ? null : (SqlTable)left.Clone(context),
                                  right==null ? null : (SqlTable)right.Clone(context),
                                  Expression==null ? null : (SqlExpression)Expression.Clone(context));

      context.NodeMapping[this] = clone;
      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    public virtual IEnumerator<SqlTable> GetEnumerator()
    {
      foreach (SqlTable source in left)
        yield return source;

      foreach (SqlTable source in right)
        yield return source;

      yield break;
    }

    // Constructor

    internal SqlJoinExpression(SqlJoinType joinType, SqlTable left, SqlTable right, SqlExpression expression) : base(SqlNodeType.Join)
    {
      this.joinType = joinType;
      this.left = left;
      this.right = right;
      this.expression = expression;
    }
  }
}

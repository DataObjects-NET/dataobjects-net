// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlJoinExpression : SqlNode
  {
    /// <summary>
    /// Gets the type of the join.
    /// </summary>
    /// <value>The type of the join.</value>
    public SqlJoinType JoinType { get; private set; }

    /// <summary>
    /// Gets the left.
    /// </summary>
    /// <value>The left.</value>
    public SqlTable Left { get; private set; }

    /// <summary>
    /// Gets the right.
    /// </summary>
    /// <value>The right.</value>
    public SqlTable Right { get; private set; }

    /// <summary>
    /// Gets the expression.
    /// </summary>
    /// <value>The expression.</value>
    public SqlExpression Expression { get; private set; }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      var clone = new SqlJoinExpression(JoinType,
        Left==null ? null : (SqlTable) Left.Clone(context),
        Right==null ? null : (SqlTable) Right.Clone(context),
        Expression==null ? null : (SqlExpression) Expression.Clone(context));

      context.NodeMapping[this] = clone;
      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    public virtual IEnumerator<SqlTable> GetEnumerator()
    {
      foreach (SqlTable source in Left)
        yield return source;

      foreach (SqlTable source in Right)
        yield return source;

      yield break;
    }

    // Constructor

    internal SqlJoinExpression(SqlJoinType joinType, SqlTable left, SqlTable right, SqlExpression expression)
      : base(SqlNodeType.Join)
    {
      JoinType = joinType;
      Left = left;
      Right = right;
      Expression = expression;
    }
  }
}

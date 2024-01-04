// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlQueryExpression
    : SqlStatement,
      ISqlQueryExpression
  {
    private readonly ISqlQueryExpression left;
    private readonly ISqlQueryExpression right;
    private readonly bool all;

    public ISqlQueryExpression Left
    {
      get { return left; }
    }

    public ISqlQueryExpression Right
    {
      get { return right; }
    }

    public bool All
    {
      get { return all; }
    }

    internal override SqlQueryExpression Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlQueryExpression(t.NodeType,
          (ISqlQueryExpression)((SqlNode) t.left).Clone(c),
          (ISqlQueryExpression)((SqlNode) t.right).Clone(c), t.all));

    #region IEnumerable<ISqlQueryExpression> Members

    public IEnumerator<ISqlQueryExpression> GetEnumerator()
    {
      foreach (ISqlQueryExpression expression in left)
        yield return expression;

      foreach (ISqlQueryExpression expression in right)
        yield return expression;

      yield break;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable<ISqlQueryExpression>)this).GetEnumerator();
    }

    #endregion

    #region ISqlCompileUnit Members

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }

    #endregion

    #region ISqlQueryExpression Members

    public SqlQueryExpression Except(ISqlQueryExpression operand)
    {
      return SqlDml.Except(this, operand);
    }

    public SqlQueryExpression ExceptAll(ISqlQueryExpression operand)
    {
      return SqlDml.ExceptAll(this, operand);
    }

    public SqlQueryExpression Intersect(ISqlQueryExpression operand)
    {
      return SqlDml.Intersect(this, operand);
    }

    public SqlQueryExpression IntersectAll(ISqlQueryExpression operand)
    {
      return SqlDml.IntersectAll(this, operand);
    }

    public SqlQueryExpression Union(ISqlQueryExpression operand)
    {
      return SqlDml.Union(this, operand);
    }

    public SqlQueryExpression UnionAll(ISqlQueryExpression operand)
    {
      return SqlDml.UnionAll(this, operand);
    }

    #endregion

    // Constructor

    internal SqlQueryExpression(SqlNodeType nodeType, ISqlQueryExpression left, ISqlQueryExpression right, bool all)
      : base(nodeType)
    {
      this.left = left;
      this.right = right;
      this.all = all;
    }
  }
}
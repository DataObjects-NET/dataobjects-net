// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Xtensive.Sql.Dom.Dml
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

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      SqlQueryExpression clone = new SqlQueryExpression(NodeType, left, right, all);
      context.NodeMapping[this] = clone;
      return clone;
    }

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
      return Sql.Except(this, operand);
    }

    public SqlQueryExpression ExceptAll(ISqlQueryExpression operand)
    {
      return Sql.ExceptAll(this, operand);
    }

    public SqlQueryExpression Intersect(ISqlQueryExpression operand)
    {
      return Sql.Intersect(this, operand);
    }

    public SqlQueryExpression IntersectAll(ISqlQueryExpression operand)
    {
      return Sql.IntersectAll(this, operand);
    }

    public SqlQueryExpression Union(ISqlQueryExpression operand)
    {
      return Sql.Union(this, operand);
    }

    public SqlQueryExpression UnionAll(ISqlQueryExpression operand)
    {
      return Sql.UnionAll(this, operand);
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
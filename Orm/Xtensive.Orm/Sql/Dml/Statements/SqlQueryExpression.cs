// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Xtensive.Sql.Dml
{
  public class SqlQueryExpression
    : SqlStatement,
      ISqlQueryExpression
  {
    public ISqlQueryExpression Left { get; }

    public ISqlQueryExpression Right { get; }

    public bool All { get; }

    internal override SqlQueryExpression Clone(SqlNodeCloneContext context) =>
      context.GetOrAdd(this, static (t, c) =>
        new SqlQueryExpression(t.NodeType,
          (ISqlQueryExpression)((SqlNode) t.Left).Clone(c),
          (ISqlQueryExpression)((SqlNode) t.Right).Clone(c), t.All));

    #region IEnumerable<ISqlQueryExpression> Members

    public IEnumerator<ISqlQueryExpression> GetEnumerator()
    {
      foreach (ISqlQueryExpression expression in Left)
        yield return expression;

      foreach (ISqlQueryExpression expression in Right)
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
      Left = left;
      Right = right;
      All = all;
    }
  }
}
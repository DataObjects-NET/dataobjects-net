// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xtensive.Core;

namespace Xtensive.Sql.Dom.Dml
{
  [Serializable]
  public class SqlRow: SqlExpression,
    IList<SqlExpression>
  {
    private IList<SqlExpression> expressions;

    public void Add(SqlExpression item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      expressions.Add(item);
    }

    public void Clear()
    {
      expressions.Clear();
    }

    public bool Contains(SqlExpression item)
    {
      return expressions.Contains(item);
    }

    public void CopyTo(SqlExpression[] array, int index)
    {
      expressions.CopyTo(array, index);
    }

    public IEnumerator<SqlExpression> GetEnumerator()
    {
      return expressions.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return expressions.GetEnumerator();
    }

    public int IndexOf(SqlExpression item)
    {
      return expressions.IndexOf(item);
    }

    public void Insert(int index, SqlExpression item)
    {
      expressions.Insert(index, item);
    }

    public bool Remove(SqlExpression item)
    {
      return expressions.Remove(item);
    }

    public void RemoveAt(int index)
    {
      expressions.RemoveAt(index);
    }

    public int Count
    {
      get { return expressions.Count; }
    }

    public bool IsReadOnly
    {
      get { return false; }
    }

    public SqlExpression this[int index]
    {
      get { return expressions[index]; }
      set
      {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        expressions[index] = value;
      }
    }

    public override void ReplaceWith(SqlExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentIs<SqlRow>(expression, "expression");
      SqlRow replacingExpression = expression as SqlRow;
      expressions.Clear();
      foreach (SqlExpression e in replacingExpression)
        expressions.Add(e);
    }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      Collection<SqlExpression> expressionsClone = new Collection<SqlExpression>();
      foreach (SqlExpression e in expressions)
        expressionsClone.Add((SqlExpression)e.Clone(context));

      SqlRow clone = new SqlRow(expressionsClone);
      return clone;
    }

    internal SqlRow(IList<SqlExpression> expressions)
      : base(SqlNodeType.Row)
    {
      this.expressions = expressions;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {
      visitor.Visit(this);
    }
  }
}
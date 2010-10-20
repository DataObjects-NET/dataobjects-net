// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.09.01

using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  public abstract class SqlExpressionList : SqlExpression, IList<SqlExpression>
  {
    protected IList<SqlExpression> expressions;

    /// <inheritdoc/>
    public SqlExpression this[int index]
    {
      get { return expressions[index]; }
      set
      {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        expressions[index] = value;
      }
    }

    /// <inheritdoc/>
    public int Count
    {
      get { return expressions.Count; }
    }

    /// <inheritdoc/>
    public bool IsReadOnly
    {
      get { return false; }
    }

    /// <inheritdoc/>
    public void Add(SqlExpression item)
    {
      ArgumentValidator.EnsureArgumentNotNull(item, "item");
      expressions.Add(item);
    }

    /// <inheritdoc/>
    public void Clear()
    {
      expressions.Clear();
    }

    /// <inheritdoc/>
    public bool Contains(SqlExpression item)
    {
      return expressions.Contains(item);
    }

    /// <inheritdoc/>
    public void CopyTo(SqlExpression[] array, int index)
    {
      expressions.CopyTo(array, index);
    }

    /// <inheritdoc/>
    public IEnumerator<SqlExpression> GetEnumerator()
    {
      return expressions.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return expressions.GetEnumerator();
    }

    /// <inheritdoc/>
    public int IndexOf(SqlExpression item)
    {
      return expressions.IndexOf(item);
    }

    /// <inheritdoc/>
    public void Insert(int index, SqlExpression item)
    {
      expressions.Insert(index, item);
    }

    /// <inheritdoc/>
    public bool Remove(SqlExpression item)
    {
      return expressions.Remove(item);
    }

    /// <inheritdoc/>
    public void RemoveAt(int index)
    {
      expressions.RemoveAt(index);
    }


    // Constructor

    protected SqlExpressionList(SqlNodeType nodeType, IList<SqlExpression> list)
      : base(nodeType)
    {
      expressions = list;
    }
  }
}
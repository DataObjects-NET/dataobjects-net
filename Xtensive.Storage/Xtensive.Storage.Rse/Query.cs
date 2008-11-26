// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.25

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Xtensive.Storage.Rse
{
  public sealed class Query<T> : IOrderedQueryable<T>
  {
    private readonly IQueryProvider provider;
    private readonly Expression expression;

    Expression IQueryable.Expression
    {
      get { return expression; }
    }

    Type IQueryable.ElementType
    {
      get { return typeof (T); }
    }

    IQueryProvider IQueryable.Provider
    {
      get { return provider; }
    }

    public IEnumerator<T> GetEnumerator()
    {
      return ((IEnumerable<T>)provider.Execute(expression)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public override string ToString()
    {
      if (expression.NodeType==ExpressionType.Constant && ((ConstantExpression) expression).Value==this)
        return string.Format("Query({0})", typeof (T));
      return expression.ToString();
    }


    // Constructors

    public Query(IQueryProvider provider)
    {
      if (provider == null)
      {
        throw new ArgumentNullException("provider");
      }
      this.provider = provider;
      expression = Expression.Constant(this);
    }

    public Query(IQueryProvider provider, Expression expression)
    {
      if (provider==null)
        throw new ArgumentNullException("provider");
      if (expression==null)
        throw new ArgumentNullException("expression");
      if (!typeof (IQueryable<T>).IsAssignableFrom(expression.Type))
        throw new ArgumentOutOfRangeException("expression");
      this.provider = provider;
      this.expression = expression;
    }
  }
}
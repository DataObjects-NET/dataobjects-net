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
using Xtensive.Core;
using Xtensive.Storage.Linq;

namespace Xtensive.Storage
{
  /// <summary>
  /// An implementation of <see cref="IQueryable{T}"/>.
  /// </summary>
  /// <typeparam name="T">The type of the content item of the data source.</typeparam>
  public sealed class Query<T> : IQueryable<T>
  {
    private static readonly IQueryProvider provider = new QueryProvider();
    private readonly Expression expression;

    /// <inheritdoc/>
    Expression IQueryable.Expression
    {
      get { return expression; }
    }

    /// <inheritdoc/>
    Type IQueryable.ElementType
    {
      get { return typeof (T); }
    }

    /// <inheritdoc/>
    IQueryProvider IQueryable.Provider
    {
      get { return provider; }
    }

    #region IEnumerable<...> members

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
      var result = (IEnumerable) (this as IQueryable<T>).Provider.Execute(expression);
      foreach (var item in result)
        yield return (T)item;
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      // TODO: Make the output readable?
      if (expression.NodeType==ExpressionType.Constant && ((ConstantExpression) expression).Value==this)
        return string.Format("Query({0})", typeof (T));
      return expression.ToString();
    }

    // Constructor-like static members 

    public static Query<T> All {
      get {
        if (!typeof(IEntity).IsAssignableFrom(typeof(T)))
          Exceptions.InvalidArgument(typeof(T), "T");
        return new Query<T>();
      }
    }


    // Constructors

    private Query()
    {
      expression = Expression.Constant(this);
    }

    /// <exception cref="ArgumentOutOfRangeException"><paramref name="expression"/>  is out of range.</exception>
    internal Query(Expression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(provider, "provider");
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      if (!typeof (IQueryable<T>).IsAssignableFrom(expression.Type))
        throw new ArgumentOutOfRangeException("expression");
      this.expression = expression;
    }
  }
}
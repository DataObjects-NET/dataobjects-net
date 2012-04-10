// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.07.01

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;

namespace Xtensive.Orm.Linq
{
  /// <summary>
  /// An implementation of <see cref="IQueryable{T}"/>.
  /// </summary>
  /// <typeparam name="T">The type of the content item of the data source.</typeparam>
  public sealed class Queryable<T> : IOrderedQueryable<T> 
  {
    private readonly QueryProvider provider;
    private readonly Expression expression;

    /// <inheritdoc/>
    public Expression Expression { get { return expression; } }

    /// <inheritdoc/>
    public Type ElementType { get { return typeof (T); } }

    /// <inheritdoc/>
    IQueryProvider IQueryable.Provider { get { return provider; } }

    #region IEnumerable<...> members

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
      var result = provider.Execute<IEnumerable<T>>(expression);
      return result.GetEnumerator();
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
      return expression.ToString();
    }


    // Constructors

    /// <exception cref="ArgumentOutOfRangeException"><paramref name="expression"/>  is out of range.</exception>
    public Queryable(QueryProvider provider, Expression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      if (!typeof (IQueryable<T>).IsAssignableFrom(expression.Type))
        throw new ArgumentOutOfRangeException("expression");
      this.provider = provider;
      this.expression = expression;
    }
  }
}
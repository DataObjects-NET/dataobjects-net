// Copyright (C) 2009 Xtensive LLC.
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
using Xtensive.Core.Reflection;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Linq
{
  /// <summary>
  /// An implementation of <see cref="IQueryable{T}"/>.
  /// </summary>
  /// <typeparam name="T">The type of the content item of the data source.</typeparam>
  public sealed class Queryable<T> : IOrderedQueryable<T>
  {
    private readonly Expression expression;

    /// <inheritdoc/>
    public Expression Expression
    {
      get { return expression; }
    }

    /// <inheritdoc/>
    public Type ElementType
    {
      get { return typeof (T); }
    }

    /// <inheritdoc/>
    IQueryProvider IQueryable.Provider
    {
      get { return Session.Demand().Handler.Provider; }
    }

    /// <summary>
    /// Gets the <see cref="RecordSet"/> this query is compiled to.
    /// </summary>
    public RecordSet Compiled
    {
      get { return Session.Demand().Handler.Translate<T>(expression).DataSource; }
    }

    #region IEnumerable<...> members

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
      var result = Session.Demand().Handler.Execute<T>(expression);
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
      // TODO: Make the output readable?
      if (expression.NodeType==ExpressionType.Constant && ((ConstantExpression) expression).Value==this)
        return string.Format("Query<{0}>.All", typeof (T).GetShortName());
      return expression.ToString();
    }


    // Constructors

    public Queryable()
    {
      expression = Expression.Constant(this);
    }

    /// <exception cref="ArgumentOutOfRangeException"><paramref name="expression"/>  is out of range.</exception>
    public Queryable(Expression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      if (!typeof (IQueryable<T>).IsAssignableFrom(expression.Type))
        throw new ArgumentOutOfRangeException("expression");
      this.expression = expression;
    }
  }
}
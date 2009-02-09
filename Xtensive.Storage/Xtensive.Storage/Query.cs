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
using Xtensive.Core.Reflection;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage
{
  /// <summary>
  /// An implementation of <see cref="IQueryable{T}"/>.
  /// </summary>
  /// <typeparam name="T">The type of the content item of the data source.</typeparam>
  public sealed class Query<T> : IOrderedQueryable<T>
  {
    private static readonly QueryProvider provider = new QueryProvider();
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
    public IQueryProvider Provider
    {
      get { return provider; }
    }


    /// <summary>
    /// Gets the <see cref="RecordSet"/> this query is compiled to.
    /// </summary>
    public RecordSet Compiled
    {
      get { return provider.Compile(expression).RecordSet; }
    }

    #region IEnumerable<...> members

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
    {
      var result = (IEnumerable) this.Provider.Execute(expression);
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
        return string.Format("Query<{0}>.All", typeof(T).GetShortName());
      return expression.ToString();
    }

    // Constructor-like static members 

    /// <summary>
    /// The "starting point" for any LINQ query -
    /// a <see cref="IQueryable{T}"/> enumerating all the instances
    /// of type <typeparamref name="T"/> (<typeparamref name="T"/>
    /// must be assignable to <see cref="Entity"/> or <see cref="IEntity"/> type).
    /// </summary>
    public static IQueryable<T> All {
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
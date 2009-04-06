// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.26

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Linq
{
  /// <summary>
  /// Base class for <see cref="IQueryProvider"/> implementation.
  /// </summary>
  internal abstract class QueryProviderBase : IQueryProvider
  {
    /// <inheritdoc/>
    IQueryable IQueryProvider.CreateQuery(Expression expression)
    {
      Type elementType = TypeHelper.GetElementType(expression.Type);
      try {
        var query = (IQueryable)typeof (Query<>).Activate(new[] {elementType}, new object[] {expression});
        return query;
      }
      catch (TargetInvocationException tie) {
        throw tie.InnerException;
      }
    }

    /// <inheritdoc/>
    IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expression)
    {
      return new Query<TElement>(expression);
    }

    /// <inheritdoc/>
    object IQueryProvider.Execute(Expression expression)
    {
      return Execute(expression);
    }

    /// <inheritdoc/>
    TResult IQueryProvider.Execute<TResult>(Expression expression)
    {
      object execute = Execute(expression);
      execute = execute ?? default(TResult);
      return (TResult)execute;
    }

    /// <summary>
    /// Executes the query represented by a specified expression tree.
    /// </summary>
    /// <param name="expression">An expression tree that represents a LINQ query.</param>
    /// <returns>
    /// The value that results from executing the specified query.
    /// </returns>
    protected abstract object Execute(Expression expression);
    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected QueryProviderBase()
    {
    }
  }
}
// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.26

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Linq
{
  /// <summary>
  /// <see cref="IQueryProvider"/> implementation.
  /// </summary>
  internal sealed class QueryProvider : IQueryProvider
  {
    private static readonly QueryProvider instance = new QueryProvider();

    /// <summary>
    /// Gets the only instance of this provider.
    /// </summary>
    public static QueryProvider Instance
    {
      get { return instance; }
    }

    /// <inheritdoc/>
    IQueryable IQueryProvider.CreateQuery(Expression expression)
    {
      Type elementType = SequenceHelper.GetElementType(expression.Type);
      try {
        var query = (IQueryable) typeof (Queryable<>).Activate(new[] {elementType}, new object[] {expression});
        return query;
      }
      catch (TargetInvocationException e) {
        throw e.InnerException;
      }
    }

    /// <inheritdoc/>
    IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expression)
    {
      return new Queryable<TElement>(expression);
    }

    /// <inheritdoc/>
    object IQueryProvider.Execute(Expression expression)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    public TResult Execute<TResult>(Expression expression)
    {
      var query = Translate<TResult>(expression);
      return query.Execute();
    }

    internal TranslatedQuery<TResult> Translate<TResult>(Expression expression)
    {
      var model = Domain.Demand().Model;
      var context = new TranslatorContext(expression, model);
      return context.Translator.Translate<TResult>();
    }


    // Constructors

    private QueryProvider()
    {
    }
  }
}

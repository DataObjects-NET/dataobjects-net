// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.26

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Parameters;
using Xtensive.Reflection;
using Xtensive.Linq;
using Xtensive.Orm.Internals;

namespace Xtensive.Orm.Linq
{
  /// <summary>
  /// <see cref="IQueryProvider"/> implementation.
  /// </summary>
  public sealed class QueryProvider : IQueryProvider
  {
    private readonly Session session;

    public Session Session
    {
      get { return session; }
    }
    
    /// <inheritdoc/>
    IQueryable IQueryProvider.CreateQuery(Expression expression)
    {
      Type elementType = SequenceHelper.GetElementType(expression.Type);
      try {
        var query = (IQueryable) typeof (Queryable<>).Activate(new[] {elementType}, new object[] {this, expression});
        return query;
      }
      catch (TargetInvocationException e) {
        throw e.InnerException;
      }
    }

    /// <inheritdoc/>
    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
      return new Queryable<TElement>(this, expression);
    }

    /// <inheritdoc/>
    object IQueryProvider.Execute(Expression expression)
    {
      expression = session.Events.NotifyQueryExecuting(expression);
      var resultType = expression.Type.IsOfGenericInterface(typeof (IEnumerable<>))
        ? typeof (IEnumerable<>).MakeGenericType(expression.Type.GetGenericArguments())
        : expression.Type;
      try {
        var executeMethod = WellKnownMembers.QueryProvider.Execute.MakeGenericMethod(resultType);
          var result = executeMethod.Invoke(this, new[] {expression});
          session.Events.NotifyQueryExecuted(expression);
          return result;
      }
      catch(TargetInvocationException te) {
        throw te.InnerException;
      }
    }

    /// <inheritdoc/>
    public TResult Execute<TResult>(Expression expression)
    {
      expression = session.Events.NotifyQueryExecuting(expression);
      var translationResult = Translate<TResult>(expression);
      var cachingScope = QueryCachingScope.Current;
      TResult result;
      if (cachingScope != null && !cachingScope.Execute)
        result= default(TResult);
      else
        result = translationResult.Query.Execute(session, new ParameterContext());
      session.Events.NotifyQueryExecuted(expression);
      return result;
    }

    public TranslationResult<TResult> Translate<TResult>(Expression expression)
    {
      try {
        var context = new TranslatorContext(expression, session.Domain);
        return context.Translator.Translate<TResult>();
      }
      catch (Exception ex) {
        throw new QueryTranslationException(String.Format(Resources.Strings.ExUnableToTranslateXExpressionSeeInnerExceptionForDetails, expression.ToString(true)), ex);
      }
    }


    // Constructors

    internal QueryProvider(Session session)
    {
      this.session = session;
    }
  }
}
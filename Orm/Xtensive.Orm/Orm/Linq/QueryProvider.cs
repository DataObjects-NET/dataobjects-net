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
using Xtensive.Orm.Rse.Compilation;
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

    /// <summary>
    /// Gets <see cref="Session"/> this provider is attached to.
    /// </summary>
    public Session Session { get { return session; } }
    
    
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


    /// <summary>
    /// Creates the query.
    /// </summary>
    /// <typeparam name="TElement">The type of the element.</typeparam>
    /// <param name="expression">The expression.</param>
    /// <returns></returns>
    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
      return new Queryable<TElement>(this, expression);
    }

    
    object IQueryProvider.Execute(Expression expression)
    {
      var resultType = expression.Type.IsOfGenericInterface(typeof (IEnumerable<>))
        ? typeof (IEnumerable<>).MakeGenericType(expression.Type.GetGenericArguments())
        : expression.Type;
      try {
        var executeMethod = WellKnownMembers.QueryProvider.Execute.MakeGenericMethod(resultType);
        var result = executeMethod.Invoke(this, new[] {expression});
        return result;
      }
      catch(TargetInvocationException te) {
        throw te.InnerException;
      }
    }


    /// <summary>
    /// Executes the specified expression.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="expression">The expression.</param>
    /// <returns></returns>
    public TResult Execute<TResult>(Expression expression)
    {
      expression = session.Events.NotifyQueryExecuting(expression);
      var query = Translate<TResult>(expression);
      var cachingScope = QueryCachingScope.Current;
      TResult result;
      if (cachingScope != null && !cachingScope.Execute)
        result = default(TResult);
      else
        result = query.Execute(session, new ParameterContext());
      session.Events.NotifyQueryExecuted(expression);
      return result;
    }

    #region Private / internal methods

    internal TranslatedQuery<TResult> Translate<TResult>(Expression expression)
    {
      return Translate<TResult>(expression, new CompilerConfiguration());
    }

    internal TranslatedQuery<TResult> Translate<TResult>(Expression expression, CompilerConfiguration compilerConfiguration)
    {
      try {
        var context = new TranslatorContext(session.Domain, compilerConfiguration, expression);
        return context.Translator.Translate<TResult>();
      }
      catch (Exception ex) {
        throw new QueryTranslationException(String.Format(
          Strings.ExUnableToTranslateXExpressionSeeInnerExceptionForDetails, expression.ToString(true)), ex);
      }
    }

    #endregion


    // Constructors

    internal QueryProvider(Session session)
    {
      this.session = session;
    }
  }
}
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
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Providers;
using Xtensive.Reflection;

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

    /// <inheritdoc/>
    public TResult Execute<TResult>(Expression expression)
    {
      expression = session.Events.NotifyQueryExecuting(expression);
      var query = Translate<TResult>(expression);
      var cachingScope = QueryCachingScope.Current;
      TResult result;
      if (cachingScope != null && !cachingScope.Execute)
        result = default(TResult);
      else {
        try {
          result = query.Execute(session, new ParameterContext());
        }
        catch (Exception exception) {
          session.Events.NotifyQueryExecuted(expression, exception);
          throw;
        }
      }
      session.Events.NotifyQueryExecuted(expression);
      return result;
    }

    public Task<TResult> ExecuteAsync<TResult>(Expression expression)
    {
      return ExecuteAsync<TResult>(expression, CancellationToken.None);
    }

    public async Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken token)
    {
      expression = session.Events.NotifyQueryExecuting(expression);
      var query = Translate<TResult>(expression);
      var cachingScope = QueryCachingScope.Current;
      TResult result;
      if (cachingScope != null && !cachingScope.Execute)
        result = default(TResult);
      else {
        try {
          result = await query.ExecuteAsync(session, new ParameterContext(), token).ConfigureAwait(false);
        }
        catch (Exception exception) {
          session.Events.NotifyQueryExecuted(expression, exception);
          throw;
        }
      }
      session.Events.NotifyQueryExecuted(expression);
      return result;
    }

    #region Private / internal methods

    internal TranslatedQuery<TResult> Translate<TResult>(Expression expression)
    {
      return Translate<TResult>(expression, session.CompilationService.CreateConfiguration(session));
    }

    internal TranslatedQuery<TResult> Translate<TResult>(Expression expression, CompilerConfiguration compilerConfiguration)
    {
      try {
        var context = new TranslatorContext(session, compilerConfiguration, expression);
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
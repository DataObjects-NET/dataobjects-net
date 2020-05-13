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
using System.Runtime.ExceptionServices;
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
    internal QueryCachingScope QueryCachingScope;

    /// <summary>
    /// Gets <see cref="Session"/> this provider is attached to.
    /// </summary>
    public Session Session => session;

    /// <inheritdoc/>
    IQueryable IQueryProvider.CreateQuery(Expression expression)
    {
      var elementType = SequenceHelper.GetElementType(expression.Type);
      try {
        var query = (IQueryable) WellKnown.Types.QueryableOfT.Activate(new[] {elementType}, this, expression);
        return query;
      }
      catch (TargetInvocationException e) {
        if (e.InnerException != null) {
          ExceptionDispatchInfo.Throw(e.InnerException);
        }

        throw;
      }
    }

    /// <inheritdoc/>
    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) =>
      new Queryable<TElement>(this, expression);

    /// <inheritdoc/>
    object IQueryProvider.Execute(Expression expression)
    {
      var iEnumerableOfT = WellKnown.Interfaces.EnumerableOfT;
      var resultType = expression.Type.IsOfGenericInterface(iEnumerableOfT)
        ? iEnumerableOfT.MakeGenericType(expression.Type.GetGenericArguments())
        : expression.Type;
      try {
        var executeMethod = WellKnownMembers.QueryProvider.Execute.MakeGenericMethod(resultType);
        var result = executeMethod.Invoke(this, new object[] {expression});
        return result;
      }
      catch (TargetInvocationException e) {
        if (e.InnerException != null) {
          ExceptionDispatchInfo.Throw(e.InnerException);
        }

        throw;
      }
    }

    /// <inheritdoc/>
    public TResult Execute<TResult>(Expression expression)
    {
      expression = session.Events.NotifyQueryExecuting(expression);
      var query = Translate<TResult>(expression);
      TResult result;
      if (QueryCachingScope != null && !QueryCachingScope.Execute) {
        result = default;
      }
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

    public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken token) =>
      ExecuteAsync<TResult>(expression, token, false);

    public Task<IAsyncEnumerable<T>> ExecuteForEnumerationAsync<T>(Expression expression, CancellationToken token) =>
      ExecuteAsync<IAsyncEnumerable<T>>(expression, token, true);

    private async Task<TResult> ExecuteAsync<TResult>(
      Expression expression, CancellationToken token, bool isAsyncEnumeration)
    {
      expression = session.Events.NotifyQueryExecuting(expression);
      var query = Translate<TResult>(expression, isAsyncEnumeration);
      TResult result;
      if (QueryCachingScope != null && !QueryCachingScope.Execute) {
        result = default;
      }
      else {
        try {
          result = await query
            .ExecuteAsync(session, new ParameterContext(), isAsyncEnumeration, token)
            .ConfigureAwait(false);
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

    internal TranslatedQuery<TResult> Translate<TResult>(Expression expression, bool isAsync = false) =>
      Translate<TResult>(expression, session.CompilationService.CreateConfiguration(session), isAsync);

    internal TranslatedQuery<TResult> Translate<TResult>(Expression expression,
      CompilerConfiguration compilerConfiguration, bool isAsync = false)
    {
      try {
        var context = new TranslatorContext(session, compilerConfiguration, expression, QueryCachingScope, isAsync);
        return context.Translator.Translate<TResult>();
      }
      catch (Exception ex) {
        throw new QueryTranslationException(string.Format(
          Strings.ExUnableToTranslateXExpressionSeeInnerExceptionForDetails, expression.ToString(true)), ex);
      }
    }

    #endregion


    // Constructors

    internal QueryProvider(Session session, QueryCachingScope queryCachingScope = null)
    {
      this.session = session;
      this.QueryCachingScope = queryCachingScope;
    }
  }
}
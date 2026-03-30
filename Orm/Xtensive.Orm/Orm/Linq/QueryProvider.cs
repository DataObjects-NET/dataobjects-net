// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kochetov
// Created:    2008.11.26

using System;
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
    /// <summary>
    /// Gets <see cref="Session"/> this provider is attached to.
    /// </summary>
    public Session Session { get; }

    /// <inheritdoc/>
    IQueryable IQueryProvider.CreateQuery(Expression expression)
    {
      var elementType = SequenceHelper.GetElementType(expression.Type);
      try {
        var query = (IQueryable) WellKnownTypes.QueryableOfT.Activate(new[] {elementType}, this, expression);
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
      var resultType = expression.Type;
#if NET8_0_OR_GREATER
      var executeMethod = resultType.IsOfGenericInterface(WellKnownInterfaces.EnumerableOfT)
        ? WellKnownMembers.QueryProvider.ExecuteSequence.CachedMakeGenericMethodInvoker(SequenceHelper.GetElementType(resultType))
        : WellKnownMembers.QueryProvider.ExecuteScalar.CachedMakeGenericMethodInvoker(resultType);
      try {
        return executeMethod.Invoke(this, expression);
      }
#else
      var executeMethod = resultType.IsOfGenericInterface(WellKnownInterfaces.EnumerableOfT)
        ? WellKnownMembers.QueryProvider.ExecuteSequence.CachedMakeGenericMethod(SequenceHelper.GetElementType(resultType))
        : WellKnownMembers.QueryProvider.ExecuteScalar.CachedMakeGenericMethod(resultType);
      try {
        return executeMethod.Invoke(this, new object[] { expression });
      }
#endif
      catch (TargetInvocationException e) {
        if (e.InnerException != null) {
          ExceptionDispatchInfo.Throw(e.InnerException);
        }

        throw;
      }
    }

    /// <inheritdoc/>
    public TResult Execute<TResult>(Expression expression) => ExecuteScalar<TResult>(expression);

    internal TResult ExecuteScalar<TResult>(Expression expression)
    {
      static TResult ExecuteScalarQuery(TranslatedQuery query, Session session, ParameterContext parameterContext)
      {
        return query.ExecuteScalar<TResult>(session, parameterContext);
      }

      return Execute(expression, ExecuteScalarQuery);
    }

    internal QueryResult<T> ExecuteSequence<T>(Expression expression)
    {
      static QueryResult<T> ExecuteSequenceQuery(
        TranslatedQuery query, Session session, ParameterContext parameterContext)
      {
        return query.ExecuteSequence<T>(session, parameterContext);
      }

      return Execute(expression, ExecuteSequenceQuery);
    }

    private TResult Execute<TResult>(Expression expression,
      Func<TranslatedQuery, Session, ParameterContext, TResult> runQuery)
    {
      expression = Session.Events.NotifyQueryExecuting(expression);
      var query = Translate(expression);
      TResult result;
      var compiledQueryScope = CompiledQueryProcessingScope.Current;
      if (compiledQueryScope != null && !compiledQueryScope.Execute) {
        result = default;
      }
      else {
        try {
          result = runQuery(query, Session, compiledQueryScope?.ParameterContext ?? new ParameterContext());
        }
        catch (Exception exception) {
          Session.Events.NotifyQueryExecuted(expression, exception);
          throw;
        }
      }
      Session.Events.NotifyQueryExecuted(expression);
      return result;
    }

    internal Task<TResult> ExecuteScalarAsync<TResult>(Expression expression, CancellationToken token)
    {
      static Task<TResult> ExecuteScalarQueryAsync(
        TranslatedQuery query, Session session, ParameterContext parameterContext, CancellationToken token)
      {
        return query.ExecuteScalarAsync<TResult>(session, parameterContext, token);
      }
      return ExecuteAsync(expression, ExecuteScalarQueryAsync, token);
    }

    internal Task<QueryResult<T>> ExecuteSequenceAsync<T>(Expression expression, CancellationToken token)
    {
      static Task<QueryResult<T>> ExecuteSequenceQueryAsync(
        TranslatedQuery query, Session session, ParameterContext parameterContext, CancellationToken token)
      {
        return query.ExecuteSequenceAsync<T>(session, parameterContext, token);
      }
      return ExecuteAsync(expression, ExecuteSequenceQueryAsync, token);
    }

    private async Task<TResult> ExecuteAsync<TResult>(Expression expression,
      Func<TranslatedQuery, Session, ParameterContext, CancellationToken, Task<TResult>> runQuery, CancellationToken token)
    {
      expression = Session.Events.NotifyQueryExecuting(expression);
      var query = Translate(expression);
      TResult result;
      try {
        result = await runQuery(query, Session, new ParameterContext(), token).ConfigureAwait(false);
      }
      catch (Exception exception) {
        Session.Events.NotifyQueryExecuted(expression, exception);
        throw;
      }

      Session.Events.NotifyQueryExecuted(expression);
      return result;
    }

    internal TranslatedQuery Translate(Expression expression) =>
      Translate(expression, Session.CompilationService.CreateConfiguration(Session));

    internal TranslatedQuery Translate(Expression expression,
      CompilerConfiguration compilerConfiguration)
    {
      try {
        var compiledQueryScope = CompiledQueryProcessingScope.Current;
        var context = new TranslatorContext(Session, compilerConfiguration, expression, compiledQueryScope);
        return context.Translator.Translate();
      }
      catch (Exception ex) {
        throw new QueryTranslationException(string.Format(
          Strings.ExUnableToTranslateXExpressionSeeInnerExceptionForDetails, expression.ToString(true)), ex);
      }
    }


    // Constructors

    internal QueryProvider(Session session)
    {
      Session = session;
    }
  }
}
// Copyright (C) 2012-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2012.01.27

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Caching;
using Xtensive.Core;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Linq.Expressions.Visitors;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Reflection;

namespace Xtensive.Orm.Internals
{
  internal class CompiledQueryRunner
  {
    private static readonly Func<FieldInfo, bool> FieldIsSimple = fieldInfo => IsSimpleType(fieldInfo.FieldType);

    private readonly Domain domain;
    private readonly Session session;
    private readonly QueryEndpoint endpoint;
    private readonly object queryKey;
    private readonly object queryTarget;
    private readonly ParameterContext outerContext;

    private Parameter queryParameter;
    private ExtendedExpressionReplacer queryParameterReplacer;

    public QueryResult<TElement> ExecuteCompiled<TElement>(Func<QueryEndpoint, IQueryable<TElement>> query)
    {
      var parameterizedQuery = GetSequenceQuery(query);
      return parameterizedQuery.ExecuteSequence<TElement>(session, CreateParameterContext(parameterizedQuery));
    }

    public QueryResult<TElement> ExecuteCompiled<TElement>(Func<QueryEndpoint, IOrderedQueryable<TElement>> query)
    {
      var parameterizedQuery = GetSequenceQuery(query);
      return parameterizedQuery.ExecuteSequence<TElement>(session, CreateParameterContext(parameterizedQuery));
    }

    public TResult ExecuteCompiled<TResult>(Func<QueryEndpoint, TResult> query)
    {
      var parameterizedQuery = GetCachedQuery();
      if (parameterizedQuery!=null) {
        return parameterizedQuery.ExecuteScalar<TResult>(session, CreateParameterContext(parameterizedQuery));
      }

      GetScalarQuery(query, true, out var result);
      return result;
    }

    public Task<QueryResult<TElement>> ExecuteCompiledAsync<TElement>(
      Func<QueryEndpoint, IQueryable<TElement>> query, CancellationToken token)
    {
      var parameterizedQuery = GetSequenceQuery(query);
      token.ThrowIfCancellationRequested();
      var parameterContext = CreateParameterContext(parameterizedQuery);
      token.ThrowIfCancellationRequested();

      return parameterizedQuery.ExecuteSequenceAsync<TElement>(session, parameterContext, token);
    }

    public Task<QueryResult<TElement>> ExecuteCompiledAsync<TElement>(
      Func<QueryEndpoint, IOrderedQueryable<TElement>> query, CancellationToken token) =>
      ExecuteCompiledAsync((Func<QueryEndpoint, IQueryable<TElement>>)query, token);

    public Task<TResult> ExecuteCompiledAsync<TResult>(Func<QueryEndpoint, TResult> query, CancellationToken token)
    {
      var parameterizedQuery = GetCachedQuery();
      if (parameterizedQuery!=null) {
        token.ThrowIfCancellationRequested();
        return parameterizedQuery.ExecuteScalarAsync<TResult>(session, CreateParameterContext(parameterizedQuery), token);
      }

      parameterizedQuery = GetScalarQuery(query, false, out _);
      token.ThrowIfCancellationRequested();
      return parameterizedQuery.ExecuteScalarAsync<TResult>(session, CreateParameterContext(parameterizedQuery), token);
    }

    public DelayedScalarQuery<TResult> CreateDelayedQuery<TResult>(Func<QueryEndpoint, TResult> query)
    {
      var parameterizedQuery = GetCachedQuery() ?? GetScalarQuery(query, false, out _);
      var parameterContext = CreateParameterContext(parameterizedQuery);
      var result = new DelayedScalarQuery<TResult>(session, parameterizedQuery, parameterContext);
      session.RegisterUserDefinedDelayedQuery(result.Task);
      return result;
    }

    public DelayedQuery<TElement> CreateDelayedQuery<TElement>(Func<QueryEndpoint, IOrderedQueryable<TElement>> query) =>
      CreateDelayedSequenceQuery(query);

    public DelayedQuery<TElement> CreateDelayedQuery<TElement>(Func<QueryEndpoint, IQueryable<TElement>> query) =>
      CreateDelayedSequenceQuery(query);

    private DelayedQuery<TElement> CreateDelayedSequenceQuery<TElement>(
      Func<QueryEndpoint, IQueryable<TElement>> query)
    {
      var parameterizedQuery = GetSequenceQuery(query);
      var parameterContext = CreateParameterContext(parameterizedQuery);
      var result = new DelayedQuery<TElement>(session, parameterizedQuery, parameterContext);
      session.RegisterUserDefinedDelayedQuery(result.Task);
      return result;
    }

    private ParameterizedQuery GetScalarQuery<TResult>(
      Func<QueryEndpoint, TResult> query, bool executeAsSideEffect, out TResult result)
    {
      var cacheable = AllocateParameterAndReplacer();

      var parameterContext = new ParameterContext(outerContext);
      parameterContext.SetValue(queryParameter, queryTarget);
      var scope = new CompiledQueryProcessingScope(
        queryParameter, queryParameterReplacer, parameterContext, executeAsSideEffect);
      using (scope.Enter()) {
        result = query.Invoke(endpoint);
      }

      var parameterizedQuery = (ParameterizedQuery) scope.ParameterizedQuery;
      if (parameterizedQuery == null && queryTarget != null) {
        throw new NotSupportedException(Strings.ExNonLinqCallsAreNotSupportedWithinQueryExecuteDelayed);
      }

      cacheable = cacheable && parameterizedQuery != null && !UsesSqlTemporaryDataProvider(parameterizedQuery.DataSource);
      if (cacheable) {
        PutCachedQuery(parameterizedQuery);
      }
      return parameterizedQuery;
    }

    private ParameterizedQuery GetSequenceQuery<TElement>(
      Func<QueryEndpoint, IQueryable<TElement>> query)
    {
      var parameterizedQuery = GetCachedQuery();
      if (parameterizedQuery!=null) {
        return parameterizedQuery;
      }

      var cacheable = AllocateParameterAndReplacer();
      var scope = new CompiledQueryProcessingScope(queryParameter, queryParameterReplacer);
      using (scope.Enter()) {
        var result = query.Invoke(endpoint);
        var translatedQuery = endpoint.Provider.Translate(result.Expression);
        cacheable = cacheable && !UsesSqlTemporaryDataProvider(translatedQuery.DataSource);
        parameterizedQuery = (ParameterizedQuery) translatedQuery;
      }

      if (cacheable) {
        PutCachedQuery(parameterizedQuery);
      }
      return parameterizedQuery;
    }

    private static bool UsesSqlTemporaryDataProvider(Provider provider) =>
      provider is SqlTemporaryDataProvider || provider.Sources.Any(UsesSqlTemporaryDataProvider);

    private bool AllocateParameterAndReplacer()
    {
      if (queryTarget == null) {
        queryParameter = null;
        queryParameterReplacer = new ExtendedExpressionReplacer(e => e);
        return true;
      }

      var closureType = queryTarget.GetType();
      var parameterType = WellKnownOrmTypes.ParameterOfT.CachedMakeGenericType(closureType);
      var valueMemberInfo = parameterType.GetProperty(nameof(Parameter<object>.Value), closureType);
      queryParameter = (Parameter) System.Activator.CreateInstance(parameterType, "pClosure");
      queryParameterReplacer = new ExtendedExpressionReplacer(expression => {
        if (expression.NodeType == ExpressionType.Constant) {
          if ((expression as ConstantExpression).Value == null) {
          return null;
        }
        if (expression.Type.IsClosure()) {
          if (expression.Type==closureType) {
            return Expression.MakeMemberAccess(Expression.Constant(queryParameter, parameterType), valueMemberInfo);
          }
            else {
          throw new NotSupportedException(string.Format(
            Strings.ExExpressionDefinedOutsideOfCachingQueryClosure, expression));
        }
          }

        if (closureType.DeclaringType==null) {
            if (expression.Type.IsAssignableFrom(closureType))
            return Expression.MakeMemberAccess(Expression.Constant(queryParameter, parameterType), valueMemberInfo);
          }
        else {
            if (expression.Type.IsAssignableFrom(closureType))
            return Expression.MakeMemberAccess(Expression.Constant(queryParameter, parameterType), valueMemberInfo);
            if (expression.Type.IsAssignableFrom(closureType.DeclaringType)) {
            var memberInfo = closureType.TryGetFieldInfoFromClosure(expression.Type);
              if (memberInfo != null)
              return Expression.MakeMemberAccess(
                Expression.MakeMemberAccess(Expression.Constant(queryParameter, parameterType), valueMemberInfo),
                memberInfo);
            }
          }
        }
        return null;
      });

      return !TypeHelper.IsClosure(closureType)
        || closureType.GetFields().All(FieldIsSimple);
    }

    private static bool IsSimpleType(Type type)
    {
      var typeInfo = type.GetTypeInfo();
      if (typeInfo.IsGenericType) {
        var genericDef = typeInfo.GetGenericTypeDefinition();
        return (genericDef == WellKnownTypes.NullableOfT || genericDef.IsAssignableTo(WellKnownTypes.IReadOnlyListOfT))
          && IsSimpleType(typeInfo.GetGenericArguments()[0]);
      }
      else if (typeInfo.IsArray) {
        return IsSimpleType(typeInfo.GetElementType());
      }
      else {
        return typeInfo.IsPrimitive || typeInfo.IsEnum || type == WellKnownTypes.String || type == WellKnownTypes.Decimal;
      }
    }

    private ParameterizedQuery GetCachedQuery() =>
      domain.QueryCache.TryGetItem(queryKey, true, out var item) ? item.Second : null;

    private void PutCachedQuery(ParameterizedQuery parameterizedQuery) =>
      domain.QueryCache.Add(new Pair<object, ParameterizedQuery>(queryKey, parameterizedQuery));

    private ParameterContext CreateParameterContext(ParameterizedQuery query)
    {
      var parameterContext = new ParameterContext(outerContext);
      if (query.QueryParameter!=null) {
        parameterContext.SetValue(query.QueryParameter, queryTarget);
      }

      return parameterContext;
    }

    public CompiledQueryRunner(QueryEndpoint endpoint, object queryKey, object queryTarget, ParameterContext outerContext = null)
    {
      session = endpoint.Provider.Session;
      domain = session.Domain;

      this.endpoint = endpoint;
      this.queryTarget = queryTarget;
      this.outerContext = outerContext;

      this.queryKey = domain.Configuration.ShareQueryCacheOverNodes
        ? queryKey
        : new Pair<object, string>(queryKey, session.StorageNodeId);
    }
  }
}

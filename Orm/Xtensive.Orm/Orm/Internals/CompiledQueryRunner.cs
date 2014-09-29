// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.01.27

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Linq.Expressions.Visitors;
using Xtensive.Reflection;

namespace Xtensive.Orm.Internals
{
  internal class CompiledQueryRunner
  {
    private readonly Domain domain;
    private readonly Session session;
    private readonly QueryEndpoint endpoint;
    private readonly object queryKey;
    private readonly object queryTarget;

    private Parameter queryParameter;
    private ExtendedExpressionReplacer queryParameterReplacer;

    public IEnumerable<TElement> ExecuteCompiled<TElement>(Func<QueryEndpoint, IQueryable<TElement>> query)
    {
      var parameterizedQuery = GetSequenceQuery(query);
      return parameterizedQuery.Execute(session, CreateParameterContext(parameterizedQuery));
    }

    public TResult ExecuteCompiled<TResult>(Func<QueryEndpoint, TResult> query)
    {
      var parameterizedQuery = GetCachedQuery<TResult>();
      if (parameterizedQuery!=null)
        return parameterizedQuery.Execute(session, CreateParameterContext(parameterizedQuery));
      TResult result;
      GetScalarQuery(query, true, out result);
      return result;
    }

#if NET45
    public async Task<IEnumerable<TElement>> ExecuteCompiledAsync<TElement>(Func<QueryEndpoint, IQueryable<TElement>> query)
    {
      var parameterizedQuery = await GetSequenceQueryAsync(query);
      var result = await parameterizedQuery.ExecuteAsync(session, CreateParameterContext(parameterizedQuery));
      return result;
    }

    public async Task<TResult> ExecuteCompiledAsync<TResult>(Func<QueryEndpoint, TResult> query)
    {
      var parameterizedQuery = await GetCachedQueryAsync<TResult>();
      if (parameterizedQuery!=null)
        return await parameterizedQuery.ExecuteAsync(session, CreateParameterContext(parameterizedQuery));
      // dirty hack of async method
      // async methods cannot contains out or ref parameters
      // In synchroniously method out used because TResult can be value-type
      // But ParameterizedQuery<T> is always reference type and we can pass ref to instance without ref or out
      // To make sure that parameterizedQuery is initialized within GetScalarQueryAsync we must await result of method
      return await GetScalarQueryAsync(query, true, parameterizedQuery);
    }
#endif

    public IEnumerable<TElement> ExecuteDelayed<TElement>(Func<QueryEndpoint, IQueryable<TElement>> query)
    {
      var parameterizedQuery = GetSequenceQuery(query);
      var parameterContext = CreateParameterContext(parameterizedQuery);
      var result = new DelayedSequence<TElement>(session, parameterizedQuery, parameterContext);
      session.RegisterDelayedQuery(result.Task);
      return result;
    }

    public Delayed<TResult> ExecuteDelayed<TResult>(Func<QueryEndpoint, TResult> query)
    {
      TResult dummy;
      var parameterizedQuery = GetCachedQuery<TResult>() ?? GetScalarQuery(query, false, out dummy);
      var parameterContext = CreateParameterContext(parameterizedQuery);
      var result = new Delayed<TResult>(session, parameterizedQuery, parameterContext);
      session.RegisterDelayedQuery(result.Task);
      return result;
    }

    public IEnumerable<TElement> ExecuteDelayed<TElement>(Func<QueryEndpoint, IOrderedQueryable<TElement>> query)
    {
      var parameterizedQuery = GetSequenceQuery(query);
      var parameterContext = CreateParameterContext(parameterizedQuery);
      var result = new DelayedSequence<TElement>(session, parameterizedQuery, parameterContext);
      session.RegisterDelayedQuery(result.Task);
      return result;
    }

#if NET45
    public DelayedTask<IEnumerable<TElement>> ExecuteDelayedAsync<TElement>(Func<QueryEndpoint, IQueryable<TElement>> query)
    {
      var parameterizedQuery = GetSequenceQuery(query);
      var parameterContext = CreateParameterContext(parameterizedQuery);
      var result = new DelayedSequence<TElement>(session, parameterizedQuery, parameterContext);
      session.RegisterDelayedQuery(result.Task);
      return DelayedTask<IEnumerable<TElement>>.Factory.CreateNew(session, result);
    }

    public DelayedTask<TResult> ExecuteDelayedAsync<TResult>(Func<QueryEndpoint, TResult> query)
    {
      TResult dummy;
      var parameterizedQuery = GetCachedQuery<TResult>() ?? GetScalarQuery(query, false, out dummy);
      var parameterContext = CreateParameterContext(parameterizedQuery);
      var result = new Delayed<TResult>(session, parameterizedQuery, parameterContext);
      session.RegisterDelayedQuery(result.Task);
      return DelayedTask<TResult>.Factory.CreateNew(session, result);
    }

    public DelayedTask<IEnumerable<TElement>> ExecuteDelayedAsync<TElement>(Func<QueryEndpoint, IOrderedQueryable<TElement>> query)
    {
      var parameterizedQuery = GetSequenceQuery(query);
      var parameterContext = CreateParameterContext(parameterizedQuery);
      var result = new DelayedSequence<TElement>(session, parameterizedQuery, parameterContext);
      session.RegisterDelayedQuery(result.Task);
      return DelayedTask<IEnumerable<TElement>>.Factory.CreateNew(session, result);
    }
#endif

    private ParameterizedQuery<TResult> GetScalarQuery<TResult>(
      Func<QueryEndpoint, TResult> query, bool executeAsSideEffect, out TResult result)
    {
      AllocateParameterAndReplacer();
      using (var scope = new QueryCachingScope(queryParameter, queryParameterReplacer, executeAsSideEffect)) {
        using (new ParameterContext().Activate()) {
          if (queryParameter!=null)
            queryParameter.Value = queryTarget;
          result = query.Invoke(endpoint);
        }
        var parameterizedQuery = (ParameterizedQuery<TResult>) scope.ParameterizedQuery;
        if (parameterizedQuery==null && queryTarget!=null)
          throw new NotSupportedException(Strings.ExNonLinqCallsAreNotSupportedWithinQueryExecuteDelayed);
        PutCachedQuery(parameterizedQuery);
        return parameterizedQuery;
      }
    }

#if NET45
    private async Task<TResult> GetScalarQueryAsync<TResult>(
      Func<QueryEndpoint, TResult> query, bool executeAsSideEffects, ParameterizedQuery<TResult> parameterizedQuery)
    {
      await AllocateParameterAndReplacerAsync();
      TResult result;
      using (var scope = new QueryCachingScope(queryParameter, queryParameterReplacer, executeAsSideEffects)) {
        using (new ParameterContext().Activate()) {
          if (queryParameter!=null)
            queryParameter.Value = queryTarget;
          result = query.Invoke(endpoint);
        }
        parameterizedQuery = (ParameterizedQuery<TResult>) scope.ParameterizedQuery;
        if (parameterizedQuery==null && queryTarget!=null)
          throw new NotSupportedException(Strings.ExNonLinqCallsAreNotSupportedWithinQueryExecuteDelayed);
        await PutCachedQueryAsync(parameterizedQuery);
        return result;
      }
    }
#endif

    private ParameterizedQuery<IEnumerable<TElement>> GetSequenceQuery<TElement>(
      Func<QueryEndpoint, IQueryable<TElement>> query)
    {
      var parameterizedQuery = GetCachedQuery<IEnumerable<TElement>>();
      if (parameterizedQuery!=null)
        return parameterizedQuery;

      AllocateParameterAndReplacer();
      using (new QueryCachingScope(queryParameter, queryParameterReplacer)) {
        var result = query.Invoke(endpoint);
        var translatedQuery = endpoint.Provider.Translate<IEnumerable<TElement>>(result.Expression);
        parameterizedQuery = (ParameterizedQuery<IEnumerable<TElement>>) translatedQuery;
        PutCachedQuery(parameterizedQuery);
      }
      return parameterizedQuery;
    }

#if NET45
    private async Task<ParameterizedQuery<IEnumerable<TElement>>> GetSequenceQueryAsync<TElement>(
      Func<QueryEndpoint, IQueryable<TElement>> query)
    {
      var parameterizedQuery = await GetCachedQueryAsync<IEnumerable<TElement>>();
      if (parameterizedQuery!=null)
        return parameterizedQuery;
      await AllocateParameterAndReplacerAsync();
      using (new QueryCachingScope(queryParameter, queryParameterReplacer)) {
        var result = query.Invoke(endpoint);
        var translatedQuery = await endpoint.Provider.TranslateAsync<IEnumerable<TElement>>(result.Expression);
        parameterizedQuery = (ParameterizedQuery<IEnumerable<TElement>>) translatedQuery;
        await PutCachedQueryAsync(parameterizedQuery);
      }
      return parameterizedQuery;
    }
#endif

    private void AllocateParameterAndReplacer()
    {
      if (queryTarget==null) {
        queryParameter = null;
        queryParameterReplacer = new ExtendedExpressionReplacer(e => e);
        return;
      }

      var closureType = queryTarget.GetType();
      var parameterType = typeof (Parameter<>).MakeGenericType(closureType);
      var valueMemberInfo = parameterType.GetProperty("Value", closureType);
      queryParameter = (Parameter) System.Activator.CreateInstance(parameterType, "pClosure");
      queryParameterReplacer = new ExtendedExpressionReplacer(expression => {
        if (expression.NodeType==ExpressionType.Constant) {
          if (expression.Type.IsClosure())
            if (expression.Type==closureType) 
              return Expression.MakeMemberAccess(Expression.Constant(queryParameter, parameterType), valueMemberInfo);
            else
              throw new NotSupportedException(String.Format(
                Strings.ExExpressionDefinedOutsideOfCachingQueryClosure, expression));

          if (closureType.DeclaringType==null) {
            if (expression.Type==closureType)
              return Expression.MakeMemberAccess(Expression.Constant(queryParameter, parameterType), valueMemberInfo);
          }
          else
            if (expression.Type==closureType.DeclaringType) {
              var memberInfo = closureType.TryGetFieldInfoFromClosure(expression.Type);
              if (memberInfo!=null)
                return Expression.MakeMemberAccess(
                  Expression.MakeMemberAccess(Expression.Constant(queryParameter, parameterType), valueMemberInfo),
                  memberInfo);
            }
        }
        return null;
      });
    }

#if NET45
    private Task AllocateParameterAndReplacerAsync()
    {
      return Task.Factory.StartNew(AllocateParameterAndReplacer);
    }
#endif

    private ParameterizedQuery<TResult> GetCachedQuery<TResult>()
    {
      var cache = domain.QueryCache;
      lock (cache) {
        Pair<object, TranslatedQuery> item;
        return cache.TryGetItem(queryKey, true, out item)
          ? (ParameterizedQuery<TResult>) item.Second
          : null;
      }
    }

#if NET45
    private Task<ParameterizedQuery<TResult>> GetCachedQueryAsync<TResult>()
    {
      return Task<ParameterizedQuery<TResult>>.Factory.StartNew(
        GetCachedQuery<TResult>);
    }
#endif

    private void PutCachedQuery<TResult>(ParameterizedQuery<TResult> parameterizedQuery)
    {
      var cache = domain.QueryCache;
      lock (cache) {
        Pair<object, TranslatedQuery> item;
        if (!cache.TryGetItem(queryKey, false, out item))
          cache.Add(new Pair<object, TranslatedQuery>(queryKey, parameterizedQuery));
      }
    }

#if NET45
    private Task PutCachedQueryAsync<TResult>(ParameterizedQuery<TResult> parameterizedQuery)
    {
      return Task.Factory.StartNew(() => PutCachedQuery<TResult>(parameterizedQuery));
    }
#endif

    private ParameterContext CreateParameterContext<TResult>(ParameterizedQuery<TResult> query)
    {
      var parameterContext = new ParameterContext();
      if (query.QueryParameter!=null)
        using (parameterContext.Activate())
          query.QueryParameter.Value = queryTarget;
      return parameterContext;
    }

    public CompiledQueryRunner(QueryEndpoint endpoint, object queryKey, object queryTarget)
    {
      session = endpoint.Provider.Session;
      domain = session.Domain;

      this.endpoint = endpoint;
      this.queryKey = new Pair<object, string>(queryKey, session.StorageNodeId);
      this.queryTarget = queryTarget;
    }
  }
}
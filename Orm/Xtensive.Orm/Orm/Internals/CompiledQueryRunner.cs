// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.01.27

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Caching;
using Xtensive.Core;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Linq.Expressions.Visitors;

using Xtensive.Parameters;
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
        PutCachedQuery(parameterizedQuery);
        return parameterizedQuery;
      }
    }

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
        if (expression.NodeType==ExpressionType.Constant && expression.Type.IsClosure()) {
          if (expression.Type==closureType)
            return Expression.MakeMemberAccess(Expression.Constant(queryParameter, parameterType), valueMemberInfo);
          throw new NotSupportedException(String.Format(
            Strings.ExExpressionDefinedOutsideOfCachingQueryClosure, expression));
        }
        return null;
      });
    }

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

    private void PutCachedQuery<TResult>(ParameterizedQuery<TResult> parameterizedQuery)
    {
      var cache = domain.QueryCache;
      lock (cache) {
        Pair<object, TranslatedQuery> item;
        if (!cache.TryGetItem(queryKey, false, out item))
          cache.Add(new Pair<object, TranslatedQuery>(queryKey, parameterizedQuery));
      }
    }

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
      this.endpoint = endpoint;
      this.queryKey = queryKey;
      this.queryTarget = queryTarget;

      session = endpoint.Provider.Session;
      domain = session.Domain;
    }
  }
}
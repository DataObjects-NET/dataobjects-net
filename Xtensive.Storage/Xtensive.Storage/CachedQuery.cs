// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.24

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Parameters;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Linq.Expressions.Visitors;
using Activator=System.Activator;

namespace Xtensive.Storage
{
  /// <summary>
  /// Caches the compilation result of provided query.
  /// </summary>
  public static class CachedQuery
  {
    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="query"/> delegate 
    /// (in fact, by its <see cref="MethodInfo"/> instance)
    /// and executes them if it's already cached; 
    /// otherwise executes the <paramref name="query"/> delegate
    /// and caches the result.
    /// </summary>
    /// <typeparam name="TElement">The type of the result element.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>Query result.</returns>
    public static IEnumerable<TElement> Execute<TElement>(Func<IQueryable<TElement>> query)
    {
      var domain = Domain.Demand();
      var target = query.Target;
      var cache = domain.QueryCache;
      Pair<MethodInfo, TranslatedQuery> item;
      ParameterizedQuery<IEnumerable<TElement>> parameterizedQuery = null;
      lock (cache)
        if (cache.TryGetItem(query.Method, true, out item))
          parameterizedQuery = (ParameterizedQuery<IEnumerable<TElement>>) item.Second;
      if (parameterizedQuery == null) {
        ExtendedExpressionReplacer replacer;
        var queryParameter = BuildQueryParameter(target, out replacer);
        using (new QueryCachingScope(queryParameter, replacer)) {
          var result = query.Invoke();
          var translatedQuery = Session.Demand().Handler.Translate<TElement>(result.Expression);
          parameterizedQuery = (ParameterizedQuery<IEnumerable<TElement>>) translatedQuery;
          lock (cache)
            if (!cache.TryGetItem(query.Method, false, out item))
              cache.Add(new Pair<MethodInfo, TranslatedQuery>(query.Method, parameterizedQuery));
        }
      }
      return ExecuteSequence(parameterizedQuery, target);
    }

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="query"/> delegate
    /// (in fact, by its <see cref="MethodInfo"/> instance)
    /// and executes them if it's already cached;
    /// otherwise executes the <paramref name="query"/> delegate
    /// and caches the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>Query result.</returns>
    public static TResult Execute<TResult>(Func<TResult> query)
    {
      var domain = Domain.Demand();
      var target = query.Target;
      var cache = domain.QueryCache;
      Pair<MethodInfo, TranslatedQuery> item;
      ParameterizedQuery<TResult> parameterizedQuery = null;
      lock (cache)
        if (cache.TryGetItem(query.Method, true, out item))
          parameterizedQuery = (ParameterizedQuery<TResult>) item.Second;
      if (parameterizedQuery == null) {
        ExtendedExpressionReplacer replacer;
        var queryParameter = BuildQueryParameter(target, out replacer);
        using (var queryCachingScope = new QueryCachingScope(queryParameter, replacer)) {
          TResult result;
          using (new ParameterContext().Activate()) {
            queryParameter.Value = target;
            result = query.Invoke();
          }
          parameterizedQuery = (ParameterizedQuery<TResult>) queryCachingScope.ParameterizedQuery;
          lock (cache)
            if (!cache.TryGetItem(query.Method, false, out item))
              cache.Add(new Pair<MethodInfo, TranslatedQuery>(query.Method, parameterizedQuery));
          return result;
        }
      }
      return ExecuteScalar(parameterizedQuery, target);
    }

    #region Private methods

    private static Parameter BuildQueryParameter(object target, out ExtendedExpressionReplacer replacer)
    {
      if (target == null) {
        replacer = null;
        return null;
      }
      var closureType = target.GetType();
      var parameterType = typeof(Parameter<>).MakeGenericType(closureType);
      var valueMemberInfo = parameterType.GetProperty("Value", closureType);
      var queryParameter = (Parameter)Activator.CreateInstance(parameterType, "pClosure", target);
      replacer = new ExtendedExpressionReplacer(e =>
      {
        if (e.NodeType == ExpressionType.Constant && e.Type.IsClosure()) {
          if (e.Type == closureType)
            return Expression.MakeMemberAccess(Expression.Constant(queryParameter, parameterType), valueMemberInfo);
          throw new NotSupportedException("CachedQuery supports only queries written within its Execute methods.");
        }
        return null;
      });
      return queryParameter;
    }

    private static IEnumerable<TElement> ExecuteSequence<TElement>(ParameterizedQuery<IEnumerable<TElement>> query, object target)
    {
      using (new ParameterContext().Activate()) {
        if (query.QueryParameter != null)
          query.QueryParameter.Value = target;
        foreach (var element in query.Execute())
          yield return element;
      }
    }

    private static TResult ExecuteScalar<TResult>(ParameterizedQuery<TResult> query, object target)
    {
      using (new ParameterContext().Activate()) {
        if (query.QueryParameter != null)
          query.QueryParameter.Value = target;
        return query.Execute();
      }
    }

    #endregion
  }
}

// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.07.27

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Disposing;
using Xtensive.Core.Parameters;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Linq.Expressions.Visitors;
using Xtensive.Storage.Resources;
using Activator=System.Activator;

namespace Xtensive.Storage
{
  /// <summary>
  /// Access point to a single <see cref="Key"/> resolving.
  /// </summary>
  public static class Query
  {
    /// <summary>
    /// Resolves (gets) the <see cref="Entity"/> by the specified <paramref name="key"/>
    /// in the current <see cref="Session"/>.
    /// </summary>
    /// <param name="key">The key to resolve.</param>
    /// <returns>
    /// The <see cref="Entity"/> specified <paramref name="key"/> identifies.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Entity with the specified key is not found.</exception>
    public static Entity Single(Key key)
    {
      var session = Session.Demand();
      var result = SingleOrDefault(session, key);
      if (result == null)
        throw new KeyNotFoundException(string.Format(
          Strings.EntityWithKeyXDoesNotExist, key));
      return result;
    }

    /// <summary>
    /// Resolves (gets) the <see cref="Entity"/> by the specified <paramref name="key"/>
    /// in the current <see cref="Session"/>.
    /// </summary>
    /// <param name="key">The key to resolve.</param>
    /// <returns>
    /// The <see cref="Entity"/> specified <paramref name="key"/> identifies.
    /// <see langword="null" />, if there is no such entity.
    /// </returns>
    public static Entity SingleOrDefault(Key key)
    {
      var session = Session.Demand();
      return SingleOrDefault(session, key);
    }

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
      return Execute(query.Method, query);
    }

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="key"/>
    /// and executes them if it's already cached; 
    /// otherwise executes the <paramref name="query"/> delegate
    /// and caches the result.
    /// </summary>
    /// <typeparam name="TElement">The type of the result element.</typeparam>
    /// <param name="key">An cache item's key.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>Query result.</returns>
    public static IEnumerable<TElement> Execute<TElement>(object key, Func<IQueryable<TElement>> query)
    {
      var domain = Domain.Demand();
      var target = query.Target;
      var cache = domain.QueryCache;
      Pair<object, TranslatedQuery> item;
      ParameterizedQuery<IEnumerable<TElement>> parameterizedQuery = null;
      lock (cache)
        if (cache.TryGetItem(key, true, out item))
          parameterizedQuery = (ParameterizedQuery<IEnumerable<TElement>>) item.Second;
      if (parameterizedQuery == null) {
        ExtendedExpressionReplacer replacer;
        var queryParameter = BuildQueryParameter(target, out replacer);
        using (new QueryCachingScope(queryParameter, replacer)) {
          var result = query.Invoke();
          var translatedQuery = Session.Demand().Handler.Translate<TElement>(result.Expression);
          parameterizedQuery = (ParameterizedQuery<IEnumerable<TElement>>) translatedQuery;
          lock (cache)
            if (!cache.TryGetItem(key, false, out item))
              cache.Add(new Pair<object, TranslatedQuery>(key, parameterizedQuery));
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
      return Execute(query.Method, query);
    }

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="key"/>
    /// and executes them if it's already cached;
    /// otherwise executes the <paramref name="query"/> delegate
    /// and caches the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="key">An cache item's key.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>Query result.</returns>
    public static TResult Execute<TResult>(object key, Func<TResult> query)
    {
      var domain = Domain.Demand();
      var target = query.Target;
      var cache = domain.QueryCache;
      Pair<object, TranslatedQuery> item;
      ParameterizedQuery<TResult> parameterizedQuery = null;
      lock (cache)
        if (cache.TryGetItem(key, true, out item))
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
            if (!cache.TryGetItem(key, false, out item))
              cache.Add(new Pair<object, TranslatedQuery>(key, parameterizedQuery));
          return result;
        }
      }
      return ExecuteScalar(parameterizedQuery, target);
    }

    #region Private / internal methods

    /// <summary>
    /// Resolves the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key to resolve.</param>
    /// <param name="session">The session to resolve the <paramref name="key"/> in.</param>
    /// <returns>
    /// The <see cref="Entity"/> the specified <paramref name="key"/> identifies or <see langword="null" />.
    /// </returns>
    internal static Entity SingleOrDefault(Session session, Key key)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      using (Transaction.Open(session)) {
        var cache = session.EntityStateCache;
        var state = cache[key, true];
        bool hasBeenFetched = false;

        if (state==null) {
          if (session.IsDebugEventLoggingEnabled)
            Log.Debug("Session '{0}'. Resolving key '{1}'. Exact type is {0}.", session, key,
              key.IsTypeCached ? "known" : "unknown");
            state = session.Handler.FetchInstance(key);
          hasBeenFetched = true;
        }

        if (!hasBeenFetched && session.IsDebugEventLoggingEnabled)
          Log.Debug("Session '{0}'. Resolving key '{1}'. Key is already resolved.", session, key);

        if (state==null || state.IsNotAvailableOrMarkedAsRemoved) 
          // No state or Tuple = no data in storage
          return null;

        return state.Entity;
      }
    }

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
      var context = new ParameterContext();
      using (context.Activate()) {
        if (query.QueryParameter != null)
          query.QueryParameter.Value = target;
      }
      ParameterScope scope = null;
      var batches = query.Execute().Batch(2)
        .ApplyBeforeAndAfter(() => scope = context.Activate(), () => scope.DisposeSafely());
      foreach (var batch in batches)
        foreach (var element in batch)
          yield return element;
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
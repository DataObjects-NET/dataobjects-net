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
using Xtensive.Core.Tuples;
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
    private static readonly object indexSeekCachingRegion = new object();
    private static readonly object transformCachingRegion = new object();
    private static readonly Parameter<Tuple> seekParameter = new Parameter<Tuple>(WellKnown.KeyFieldName);

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
      return ExecuteSequence(GetParameterizedQuery(key, query, Session.Demand()), query.Target);
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

    /// <summary>
    /// Creates the future and registers it for the later execution.
    /// The query associated with the future will be cached.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="key">An cache item's key.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>The future that will be executed when its result is requested.</returns>
    public static FutureScalar<TResult> ExecuteFutureScalar<TResult>(object key,
      Expression<Func<TResult>> query)
    {
      var session = Session.Demand();
      var domain = Domain.Demand();
      var cache = domain.QueryCache;
      object target = null;
      Pair<object, TranslatedQuery> item;
      ParameterizedQuery<TResult> parameterizedQuery = null;
      lock (cache)
        if (cache.TryGetItem(key, true, out item))
          parameterizedQuery = (ParameterizedQuery<TResult>) item.Second;
      if (parameterizedQuery == null) {
        Parameter parameter;
        var preparedQuery = ReplaceClosure(query.Body, out target, out parameter);
        var transaltedQuery = session.Handler.Provider.Translate<TResult>(preparedQuery);
        parameterizedQuery = new ParameterizedQuery<TResult>(transaltedQuery, parameter);
          lock (cache)
            if (!cache.TryGetItem(key, false, out item))
              cache.Add(new Pair<object, TranslatedQuery>(key, parameterizedQuery));
      }
      else {
        var targetSearcher = new DelegatingExpressionVisitor();
        targetSearcher.Visit(query, exp => {
          if (exp.NodeType == ExpressionType.Constant && exp.Type.IsClosure())
            if (target == null)
              target = ((ConstantExpression) exp).Value;
        });
      }
      var parameterContext = CreateParameterContext(target, parameterizedQuery);
      var result = new FutureScalar<TResult>(parameterizedQuery, parameterContext);
      session.RegisterDelayedQuery(result.Task);
      return result;
    }

    /// <summary>
    /// Creates the future and registers it for the later execution.
    /// The query associated with the future will NOT be cached.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>The future that will be executed when its result is requested.</returns>
    public static FutureScalar<TResult> ExecuteFutureScalar<TResult>(Expression<Func<TResult>> query)
    {
      var session = Session.Demand();
      var translatedQuery = session.Handler.Provider.Translate<TResult>(query.Body);
      var result = new FutureScalar<TResult>(translatedQuery, null);
      session.RegisterDelayedQuery(result.Task);
      return result;
    }

    /// <summary>
    /// Creates the future and registers it for the later execution.
    /// The query associated with the future will be cached.
    /// </summary>
    /// <typeparam name="TElement">The type of the result element.</typeparam>
    /// <param name="key">An cache item's key.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>The future that will be executed when its result is requested.</returns>
    public static IEnumerable<TElement> ExecuteFuture<TElement>(object key, Func<IQueryable<TElement>> query)
    {
      var session = Session.Demand();
      var parameterizedQuery = GetParameterizedQuery(key, query, session);
      var parameterContext = CreateParameterContext(query.Target, parameterizedQuery);
      var result = new FutureSequence<TElement>(parameterizedQuery, parameterContext);
      session.RegisterDelayedQuery(result.Task);
      return result;
    }

    /// <summary>
    /// Creates the future and registers it for the later execution.
    /// The query associated with the future will be cached.
    /// </summary>
    /// <typeparam name="TElement">The type of the result element.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>The future that will be executed when its result is requested.</returns>
    public static IEnumerable<TElement> ExecuteFuture<TElement>(Func<IQueryable<TElement>> query)
    {
      return ExecuteFuture(query.Method, query);
    }

    public static IQueryable<TElement> Store<TElement>(IEnumerable<TElement> source)
    {
      var method = WellKnownMembers.Queryable.AsQueryable.MakeGenericMethod(typeof (TElement));
      var expression = Expression.Call(method, Expression.Constant(source));
      return new Queryable<TElement>(expression);
    }

    public static IQueryable<TElement> Store<TElement>(params TElement[] source)
    {
      return Store((IEnumerable<TElement>) source);
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
      Entity result;
      using (var transactionScope = Transaction.Open(session)) {
        var cache = session.EntityStateCache;
        var state = cache[key, true];
        bool hasBeenFetched = false;

        if (state==null) {
          if (session.IsDebugEventLoggingEnabled)
            Log.Debug(Strings.LogSessionXResolvingKeyYExactTypeIsZ, session, key,
              key.HasExactType ? Strings.Known : Strings.Unknown);
            state = session.Handler.FetchInstance(key);
          hasBeenFetched = true;
        }

        if (!hasBeenFetched && session.IsDebugEventLoggingEnabled)
          Log.Debug(Strings.LogSessionXResolvingKeyYKeyIsAlreadyResolved, session, key);

        if (state==null || state.IsNotAvailableOrMarkedAsRemoved) 
          // No state or Tuple = no data in storage
          result = null;
        else
          result = state.Entity;

        transactionScope.Complete();
      }
      return result;
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
          throw new NotSupportedException(Strings.CachedQuerySupportsOnlyQueriesWrittenWithinItsExecuteMethods);
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

    private static ParameterizedQuery<IEnumerable<TElement>> GetParameterizedQuery<TElement>(object key,
      Func<IQueryable<TElement>> query, Session session)
    {
      var domain = Domain.Demand();
      var cache = domain.QueryCache;
      Pair<object, TranslatedQuery> item;
      ParameterizedQuery<IEnumerable<TElement>> parameterizedQuery = null;
      lock (cache)
        if (cache.TryGetItem(key, true, out item))
          parameterizedQuery = (ParameterizedQuery<IEnumerable<TElement>>) item.Second;
      if (parameterizedQuery == null) {
        ExtendedExpressionReplacer replacer;
        var queryParameter = BuildQueryParameter(query.Target, out replacer);
        using (new QueryCachingScope(queryParameter, replacer)) {
          var result = query.Invoke();
          var translatedQuery = session.Handler.Provider.Translate<IEnumerable<TElement>>(result.Expression);
          parameterizedQuery = (ParameterizedQuery<IEnumerable<TElement>>) translatedQuery;
          lock (cache)
            if (!cache.TryGetItem(key, false, out item))
              cache.Add(new Pair<object, TranslatedQuery>(key, parameterizedQuery));
        }
      }
      return parameterizedQuery;
    }

    private static Expression ReplaceClosure(Expression source, out object target, out Parameter parameter)
    {
      Type currentClosureType = null;
      Parameter queryParameter = null;
      Type parameterType = null;
      PropertyInfo valueMemberInfo = null;
      object currentTarget = null;
      var replacer = new ExtendedExpressionReplacer(e => {
        if (e.NodeType == ExpressionType.Constant && e.Type.IsClosure()) {
          if (currentClosureType == null) {
            currentClosureType = e.Type;
            currentTarget = ((ConstantExpression) e).Value;
            parameterType = typeof (Parameter<>).MakeGenericType(currentClosureType);
            valueMemberInfo = parameterType.GetProperty("Value", currentClosureType);
            queryParameter = (Parameter) Activator.CreateInstance(parameterType, "pClosure", currentTarget);
          }
          if (e.Type == currentClosureType)
            return Expression.MakeMemberAccess(Expression.Constant(queryParameter, parameterType),
              valueMemberInfo);
          throw new InvalidOperationException(Strings.ExQueryContainsClosuresOfDifferentTypes);
        }
        return null;
      });
      var result = replacer.Replace(source);
      target = currentTarget;
      parameter = queryParameter;
      return result;
    }

    private static ParameterContext CreateParameterContext<TResult>(object target,
      ParameterizedQuery<TResult> parameterizedQuery)
    {
      ParameterContext parameterContext = null;
      if (parameterizedQuery.QueryParameter != null) {
        parameterContext = new ParameterContext();
        using (parameterContext.Activate())
          parameterizedQuery.QueryParameter.Value = target;
      }
      return parameterContext;
    }

    #endregion
  }
}
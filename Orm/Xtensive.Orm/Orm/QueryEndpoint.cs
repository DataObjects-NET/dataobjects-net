// Copyright (C) 2011-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Xtensive.Core;
using Xtensive.Orm.FullTextSearchCondition.Interfaces;
using Xtensive.Orm.FullTextSearchCondition.Nodes;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Internals.Prefetch;
using Xtensive.Orm.Linq;
using Xtensive.Reflection;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm
{
  /// <summary>
  /// Provides methods allowing to run LINQ queries,
  /// create future (delayed) and compiled queries,
  /// and finally, resolve <see cref="Key"/>s to <see cref="Entity">entities</see>.
  /// </summary>
  [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
  public sealed class QueryEndpoint
  {
    private readonly Session session;

    /// <summary>
    /// Gets outer <see cref="QueryEndpoint"/>.
    /// For root <see cref="QueryEndpoint"/> returns <see langword="null"/>.
    /// </summary>
    public QueryEndpoint Outer { get; private set; }

    /// <summary>
    /// Gets <see cref="IQueryProvider"/> implementation
    /// for this session.
    /// </summary>
    public QueryProvider Provider { get; }

    /// <summary>
    /// Gets <see cref="IQueryRootBuilder"/> associated with this instance.
    /// If <see cref="IQueryRootBuilder"/> is not set for this instance
    /// returns <see langword="null"/>.
    /// </summary>
    public IQueryRootBuilder RootBuilder { get; private set; }

    /// <summary>
    /// The "starting point" for any LINQ query -
    /// a <see cref="IQueryable{T}"/> enumerating all the instances
    /// of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type of the sequence element.</typeparam>
    /// <returns>
    /// An <see cref="IQueryable{T}"/> enumerating all the instances
    /// of type <typeparamref name="T"/>.
    /// </returns>
    public IQueryable<T> All<T>()
      where T : class, IEntity
    {
      return Provider.CreateQuery<T>(BuildRootExpression(typeof(T)));
    }

    /// <summary>
    /// The "starting point" for dynamic LINQ query -
    /// a <see cref="IQueryable"/> enumerating all the instances
    /// of type <paramref name="elementType"/>.
    /// </summary>
    /// <param name="elementType">Type of the sequence element.</param>
    /// <returns>
    /// An <see cref="IQueryable"/> enumerating all the instances
    /// of type <paramref name="elementType"/>.
    /// </returns>
    public IQueryable All(Type elementType)
    {
      return ((IQueryProvider) Provider).CreateQuery(BuildRootExpression(elementType));
    }

    #region Full-text related

    /// <summary>
    /// Performs full-text query for the text specified in free text form.
    /// </summary>
    /// <typeparam name="T">Type of the entity to query full-text index of.</typeparam>
    /// <param name="searchCriteria">The search criteria in free text form.</param>
    /// <returns>
    /// An <see cref="IQueryable{T}"/> of <see cref="FullTextMatch{T}"/>
    /// allowing to continue building the query.
    /// </returns>
    public IQueryable<FullTextMatch<T>> FreeText<T>(string searchCriteria)
      where T : Entity
    {
      ArgumentValidator.EnsureArgumentNotNull(searchCriteria, "searchCriteria");
      var method = WellKnownMembers.Query.FreeTextString.CachedMakeGenericMethod(typeof(T));
      var expression = Expression.Call(method, Expression.Constant(searchCriteria));
      return Provider.CreateQuery<FullTextMatch<T>>(expression);
    }

    /// <summary>
    /// Performs full-text query for the text specified in free text form. 
    /// Limits the result by top number of elements, sorted by rank in descending order.
    /// </summary>
    /// <typeparam name="T">Type of the entity to query full-text index of.</typeparam>
    /// <param name="searchCriteria">The search criteria in free text form.</param>
    /// <param name="topNByRank">Top number of elements to be returned.</param>
    /// <returns>
    /// An <see cref="IQueryable{T}"/> of <see cref="FullTextMatch{T}"/>
    /// allowing to continue building the query.
    /// </returns>
    public IQueryable<FullTextMatch<T>> FreeText<T>(string searchCriteria, int topNByRank) 
      where T : Entity
    {
      ArgumentValidator.EnsureArgumentNotNull(searchCriteria, "searchCriteria");
      ArgumentValidator.EnsureArgumentIsGreaterThan(topNByRank, 0, "topNByRank");
      var method = WellKnownMembers.Query.FreeTextStringTopNByRank.CachedMakeGenericMethod(typeof (T));
      var expression = Expression.Call(method, Expression.Constant(searchCriteria), Expression.Constant(topNByRank));
      return Provider.CreateQuery<FullTextMatch<T>>(expression);
    }

    /// <summary>
    /// Performs full-text query for the text specified in free text form.
    /// </summary>
    /// <typeparam name="T">Type of the entity to query full-text index of.</typeparam>
    /// <param name="searchCriteria">The search criteria in free text form.</param>
    /// <returns>
    /// An <see cref="IQueryable{T}"/> of <see cref="FullTextMatch{T}"/>
    /// allowing to continue building the query.
    /// </returns>
    public IQueryable<FullTextMatch<T>> FreeText<T>(Expression<Func<string>> searchCriteria)
      where T : Entity
    {
      ArgumentValidator.EnsureArgumentNotNull(searchCriteria, "searchCriteria");
      var method = WellKnownMembers.Query.FreeTextExpression.CachedMakeGenericMethod(typeof(T));
      var expression = Expression.Call(null, method, new[] { searchCriteria });
      return Provider.CreateQuery<FullTextMatch<T>>(expression);
    }

    /// <summary>
    /// Performs full-text query for the text specified in free text form.
    /// Limits the result by top number of elements, sorted by rank in descending order.
    /// </summary>
    /// <typeparam name="T">Type of the entity to query full-text index of.</typeparam>
    /// <param name="searchCriteria">The search criteria in free text form.</param>
    /// <param name="topNByRank">Top number of elements to be returned.</param>
    /// <returns>
    /// An <see cref="IQueryable{T}"/> of <see cref="FullTextMatch{T}"/>
    /// allowing to continue building the query.
    /// </returns>
    public IQueryable<FullTextMatch<T>> FreeText<T>(Expression<Func<string>> searchCriteria, int topNByRank)
      where T : Entity
    {
      ArgumentValidator.EnsureArgumentNotNull(searchCriteria, "searchCriteria");
      ArgumentValidator.EnsureArgumentIsGreaterThan(topNByRank, 0, "topNByRank");
      var method = WellKnownMembers.Query.FreeTextExpressionTopNByRank.CachedMakeGenericMethod(typeof (T));
      var expression = Expression.Call(null, method, searchCriteria, Expression.Constant(topNByRank));
      return Provider.CreateQuery<FullTextMatch<T>>(expression);
    }

    /// <summary>
    /// Performs full-text query for the specified search condition.
    /// </summary>
    /// <typeparam name="T">Type of the entity to query full-text index of.</typeparam>
    /// <param name="searchCriteria">Search condition.</param>
    /// <returns>
    /// An <see cref="IQueryable{T}"/> of <see cref="FullTextMatch{T}"/>
    /// allowing to continue building the query.
    /// </returns>
    public IQueryable<FullTextMatch<T>> ContainsTable<T>([NotNull] Expression<Func<ConditionEndpoint, IOperand>> searchCriteria)
      where T: Entity
    {
      ArgumentValidator.EnsureArgumentNotNull(searchCriteria, "searchCriteria");
      var method = WellKnownMembers.Query.ContainsTableExpr.CachedMakeGenericMethod(typeof (T));
      var expression = Expression.Call(null, method, searchCriteria);
      return Provider.CreateQuery<FullTextMatch<T>>(expression);
    }

    /// <summary>
    /// Performs full-text query for the specified search condition.
    /// </summary>
    /// <typeparam name="T">Type of the entity to query full-text index of.</typeparam>
    /// <param name="searchCriteria">Search condition.</param>
    /// <param name="targetFields">Fields which are included in full-text index to search over.</param>
    /// <returns>
    /// An <see cref="IQueryable{T}"/> of <see cref="FullTextMatch{T}"/>
    /// allowing to continue building the query.
    /// </returns>
    public IQueryable<FullTextMatch<T>> ContainsTable<T>(
      [NotNull] Expression<Func<ConditionEndpoint, IOperand>> searchCriteria,
      [NotNull] Expression<Func<T, object>>[] targetFields)
      where T : Entity
    {
      ArgumentValidator.EnsureArgumentNotNull(searchCriteria, "searchCriteria");
      ArgumentValidator.EnsureArgumentNotNull(targetFields, "targetFields");
      var method = WellKnownMembers.Query.ContainsTableExprWithColumns.CachedMakeGenericMethod(typeof(T));
      var expression = Expression.Call(null, method, searchCriteria, Expression.Constant(targetFields));
      return Provider.CreateQuery<FullTextMatch<T>>(expression);
    }

    /// <summary>
    /// Performs full-text query for the specified search condition.
    /// </summary>
    /// <typeparam name="T">Type of the entity to query full-text index of.</typeparam>
    /// <param name="searchCriteria">Search condition.</param>
    /// <param name="topNByRank">
    /// Specifies how many highest ranked matches (in descending order) result set should be returned.
    /// Result set may contain less number of items than specified by the parameter.
    /// </param>
    /// <returns>
    /// An <see cref="IQueryable{T}"/> of <see cref="FullTextMatch{T}"/>
    /// allowing to continue building the query.
    /// </returns>
    public IQueryable<FullTextMatch<T>> ContainsTable<T>([NotNull] Expression<Func<ConditionEndpoint, IOperand>> searchCriteria, int topNByRank)
      where T : Entity
    {
      ArgumentValidator.EnsureArgumentNotNull(searchCriteria, "searchCriteria");
      ArgumentValidator.EnsureArgumentIsGreaterThan(topNByRank, 0, "topNByRank");
      var method = WellKnownMembers.Query.ContainsTableExprTopNByRank.CachedMakeGenericMethod(typeof(T));
      var expression = Expression.Call(null, method, searchCriteria, Expression.Constant(topNByRank));
      return Provider.CreateQuery<FullTextMatch<T>>(expression);
    }

    /// <summary>
    /// Performs full-text query for the specified search condition.
    /// </summary>
    /// <typeparam name="T">Type of the entity to query full-text index of.</typeparam>
    /// <param name="searchCriteria">Search condition.</param>
    /// <param name="targetFields">Fields which are included in full-text index to search over.</param>
    /// <param name="topNByRank">
    /// Specifies how many highest ranked matches (in descending order) result set should be returned.
    /// Result set may contain less number of items than specified by the parameter.
    /// </param>
    /// <returns>
    /// An <see cref="IQueryable{T}"/> of <see cref="FullTextMatch{T}"/>
    /// allowing to continue building the query.
    /// </returns>
    public IQueryable<FullTextMatch<T>> ContainsTable<T>(
      [NotNull] Expression<Func<ConditionEndpoint, IOperand>> searchCriteria,
      [NotNull] Expression<Func<T, object>>[] targetFields,
      int topNByRank)
      where T : Entity
    {
      ArgumentValidator.EnsureArgumentNotNull(searchCriteria, "searchCriteria");
      ArgumentValidator.EnsureArgumentNotNull(targetFields, "targetFields");
      ArgumentValidator.EnsureArgumentIsGreaterThan(topNByRank, 0, "topNByRank");
      var method = WellKnownMembers.Query.ContainsTableExprTopNByRank.CachedMakeGenericMethod(typeof(T));
      var expression = Expression.Call(null, method, searchCriteria, Expression.Constant(targetFields), Expression.Constant(topNByRank));
      return Provider.CreateQuery<FullTextMatch<T>>(expression);
    }

    #endregion

    /// <summary>
    /// Resolves (gets) the <see cref="Entity"/> by the specified <paramref name="key"/>
    /// in the current <see cref="session"/>.
    /// </summary>
    /// <param name="key">The key to resolve.</param>
    /// <returns>
    /// The <see cref="Entity"/> specified <paramref name="key"/> identifies.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Entity with the specified key is not found.</exception>
    public Entity Single(Key key)
    {
      if (key==null)
        return null;
      var result = SingleOrDefault(key);
      if (result==null)
        throw new KeyNotFoundException(String.Format(
          Strings.EntityWithKeyXDoesNotExist, key));
      return result;
    }

    /// <summary>
    /// Resolves (gets) the <see cref="Entity"/> by the specified <paramref name="key"/>
    /// in the current <see cref="session"/>.
    /// </summary>
    /// <param name="key">The key to resolve.</param>
    /// <returns>
    /// The <see cref="Entity"/> specified <paramref name="key"/> identifies.
    /// <see langword="null"/>, if there is no such entity.
    /// </returns>
    [CanBeNull] public Entity SingleOrDefault(Key key)
    {
      if (key==null)
        return null;
      Entity result;
      using (var tx = session.OpenAutoTransaction()) {
        EntityState state;
        if (!session.LookupStateInCache(key, out state)) {
          if (session.IsDebugEventLoggingEnabled) {
            OrmLog.Debug(nameof(Strings.LogSessionXResolvingKeyYExactTypeIsZ), session, key, key.HasExactType ? Strings.Known : Strings.Unknown);
          }

          state = session.Handler.FetchEntityState(key);
        }
        else if (state.Tuple==null) {
          var stateKeyType = state.Key.TypeReference.Type.UnderlyingType;
          var keyType = key.TypeReference.Type.UnderlyingType;
          if (stateKeyType!=keyType && !stateKeyType.IsAssignableFrom(keyType)) {
            session.RemoveStateFromCache(state.Key, true);
            state = session.Handler.FetchEntityState(key);
          }
        }
        if (state==null || state.IsNotAvailableOrMarkedAsRemoved
          || !key.TypeReference.Type.UnderlyingType.IsAssignableFrom(state.Type.UnderlyingType))
          // No state or Tuple = null or incorrect query type => no data in storage
          result = null;
        else
          result = state.Entity;

        tx.Complete();
      }
      return result;
    }

    /// <summary>
    /// Resolves (gets) the <see cref="Entity"/> by the specified <paramref name="key"/>
    /// in the current <see cref="session"/>.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="key">The key to resolve.</param>
    /// <returns>
    /// The <see cref="Entity"/> specified <paramref name="key"/> identifies.
    /// <see langword="null"/>, if there is no such entity.
    /// </returns>
    public T Single<T>(Key key)
      where T : class, IEntity
    {
      return (T)(object)Single(key);
    }

    /// <summary>
    /// Resolves (gets) the <see cref="Entity"/> by the specified <paramref name="keyValues"/>
    /// in the current <see cref="session"/>.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="keyValues">Key values.</param>
    /// <returns>
    /// The <see cref="Entity"/> specified <paramref name="keyValues"/> identify.
    /// <see langword="null"/>, if there is no such entity.
    /// </returns>
    public T Single<T>(params object[] keyValues)
      where T : class, IEntity
    {
      return (T)(object)Single(GetKeyByValues<T>(keyValues));
    }

    /// <summary>
    /// Resolves (gets) the <see cref="Entity"/> by the specified <paramref name="key"/>
    /// in the current <see cref="session"/>.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="key">The key to resolve.</param>
    /// <returns>
    /// The <see cref="Entity"/> specified <paramref name="key"/> identifies.
    /// </returns>
    [CanBeNull] public T SingleOrDefault<T>(Key key)
      where T : class, IEntity
    {
      return (T)(object)SingleOrDefault(key);
    }

    /// <summary>
    /// Resolves (gets) the <see cref="Entity"/> by the specified <paramref name="keyValues"/>
    /// in the current <see cref="session"/>.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="keyValues">Key values.</param>
    /// <returns>
    /// The <see cref="Entity"/> specified <paramref name="keyValues"/> identify.
    /// </returns>
    [CanBeNull] public T SingleOrDefault<T>(params object[] keyValues)
      where T : class, IEntity
    {
      return (T)(object)SingleOrDefault(GetKeyByValues<T>(keyValues));
    }

    /// <summary>
    /// Fetches multiple instances of specified type  by provided <paramref name="keys"/>.
    /// </summary>
    /// <param name="keys">The source sequence.</param>
    /// <returns>The sequence of entities of type <typeparam name="T"/> matching provided <paramref name="keys"/>.</returns>
    public PrefetchQuery<T> Many<T>(IEnumerable<Key> keys)
      where T : class, IEntity
    {
      return new PrefetchQuery<T>(session, keys);
    }

    /// <summary>
    /// Fetches multiple instances of specified type  by provided <paramref name="keys"/>.
    /// </summary>
    /// <param name="keys">The source sequence.</param>
    /// <typeparam name="T">A type of entities to query; it must be a class implementing
    /// <see cref="IEntity"/> interface.</typeparam>
    /// <typeparam name="TElement">A type of keys collection elements.</typeparam>
    /// <returns>The sequence of entities of type <typeparamref name="T"/> matching provided <paramref name="keys"/>.</returns>
    public PrefetchQuery<T> Many<T, TElement>(IEnumerable<TElement> keys)
      where T : class, IEntity
    {
      var elementType = typeof (TElement);
      Func<TElement, Key> selector;
      if (elementType==WellKnownTypes.ObjectArray) {
        selector = e => Key.Create(session.Domain, session.StorageNodeId, typeof (T), TypeReferenceAccuracy.BaseType, (object[]) (object) e);
      }
      else if (WellKnownOrmTypes.Tuple.IsAssignableFrom(elementType)) {
        selector = e => Key.Create(session.Domain, session.StorageNodeId, typeof (T), TypeReferenceAccuracy.BaseType, (Tuple) (object) e);
      }
      else {
        selector = e => Key.Create(session.Domain, session.StorageNodeId, typeof (T), TypeReferenceAccuracy.BaseType, new object[] {e});
      }

      return new PrefetchQuery<T>(session, keys.Select(selector));
    }

    #region Execute methods

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="query"/> delegate
    /// (in fact, by its <see cref="MethodInfo"/> instance)
    /// and executes them if it's already cached;
    /// otherwise executes the <paramref name="query"/> delegate
    /// and caches the result.
    /// </summary>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>Query result.</returns>
    public QueryResult<TElement> Execute<TElement>(Func<QueryEndpoint, IQueryable<TElement>> query)
    {
      return new CompiledQueryRunner(this, query.Method, query.Target)
        .ExecuteCompiled(query);
    }

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="key"/>
    /// and executes them if it's already cached;
    /// otherwise executes the <paramref name="query"/> delegate
    /// and caches the result.
    /// </summary>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="key">An object identifying this query in cache.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>Query result.</returns>
    public QueryResult<TElement> Execute<TElement>(object key, Func<QueryEndpoint, IQueryable<TElement>> query)
    {
      return new CompiledQueryRunner(this, key, query.Target).ExecuteCompiled(query);
    }

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="query"/> delegate
    /// (in fact, by its <see cref="MethodInfo"/> instance)
    /// and executes them if it's already cached;
    /// otherwise executes the <paramref name="query"/> delegate
    /// and caches the result.
    /// </summary>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>Query result.</returns>
    public QueryResult<TElement> Execute<TElement>(Func<QueryEndpoint, IOrderedQueryable<TElement>> query)
    {
      return new CompiledQueryRunner(this, query.Method, query.Target).ExecuteCompiled(query);
    }

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="key"/>
    /// and executes them if it's already cached;
    /// otherwise executes the <paramref name="query"/> delegate
    /// and caches the result.
    /// </summary>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="key">An object identifying this query in cache.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>Query result.</returns>
    public QueryResult<TElement> Execute<TElement>(object key, Func<QueryEndpoint, IOrderedQueryable<TElement>> query)
    {
      return new CompiledQueryRunner(this, key, query.Target).ExecuteCompiled(query);
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
    public TResult Execute<TResult>(Func<QueryEndpoint,TResult> query)
    {
      return new CompiledQueryRunner(this, query.Method, query.Target).ExecuteCompiled(query);
    }

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="key"/>
    /// and executes them if it's already cached;
    /// otherwise executes the <paramref name="query"/> delegate
    /// and caches the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="key">An object identifying this query in cache.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>Query result.</returns>
    public TResult Execute<TResult>(object key, Func<QueryEndpoint,TResult> query)
    {
      return new CompiledQueryRunner(this, key, query.Target).ExecuteCompiled(query);
    }

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="key"/>
    /// and executes them if it's already cached;
    /// otherwise executes the <paramref name="query"/> delegate
    /// and caches the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="key">An object identifying this query in cache.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <param name="parameterContext"><see cref="ParameterContext"/> instance holding explicitly set
    /// values of query parameters.</param>
    /// <returns>Query result.</returns>
    public TResult Execute<TResult>(object key, Func<QueryEndpoint,TResult> query, ParameterContext parameterContext)
    {
      return new CompiledQueryRunner(this, key, query.Target, parameterContext).ExecuteCompiled(query);
    }

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="query"/> delegate
    /// (in fact, by its <see cref="MethodInfo"/> instance)
    /// and asynchronously executes them if it's already cached;
    /// otherwise asynchronously executes the <paramref name="query"/> delegate
    /// and caches the result.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>Task performing operation.</returns>
    public Task<QueryResult<TElement>> ExecuteAsync<TElement>(Func<QueryEndpoint, IQueryable<TElement>> query) =>
      new CompiledQueryRunner(this, query.Method, query.Target).ExecuteCompiledAsync(query, CancellationToken.None);

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="query"/> delegate
    /// (in fact, by its <see cref="MethodInfo"/> instance)
    /// and asynchronously executes them if it's already cached;
    /// otherwise asynchronously executes the <paramref name="query"/> delegate
    /// and caches the result.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <param name="token">Token to cancel operation.</param>
    /// <returns>Task performing operation.</returns>
    public Task<QueryResult<TElement>> ExecuteAsync<TElement>(Func<QueryEndpoint, IQueryable<TElement>> query, CancellationToken token) =>
      new CompiledQueryRunner(this, query.Method, query.Target).ExecuteCompiledAsync(query, token);

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="key"/>
    /// and asynchronously executes them if it's already cached;
    /// otherwise asynchronously executes the <paramref name="query"/> delegate
    /// and caches the result.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="key">An object identifying this query in cache.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>Task performing operation.</returns>
    public Task<QueryResult<TElement>> ExecuteAsync<TElement>(object key, Func<QueryEndpoint, IQueryable<TElement>> query) =>
      new CompiledQueryRunner(this, key, query.Target).ExecuteCompiledAsync(query, CancellationToken.None);

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="key"/>
    /// and asynchronously executes them if it's already cached;
    /// otherwise asynchronously executes the <paramref name="query"/> delegate
    /// and caches the result.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="key">An object identifying this query in cache.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <param name="token">Token to cancel operation.</param>
    /// <returns>Task performing operation.</returns>
    public Task<QueryResult<TElement>> ExecuteAsync<TElement>(object key, Func<QueryEndpoint, IQueryable<TElement>> query, CancellationToken token) =>
      new CompiledQueryRunner(this, key, query.Target).ExecuteCompiledAsync(query, token);

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="query"/> delegate
    /// (in fact, by its <see cref="MethodInfo"/> instance)
    /// and asynchronously executes them if it's already cached;
    /// otherwise asynchronously executes the <paramref name="query"/> delegate
    /// and caches the result.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>Task performing operation.</returns>
    public Task<QueryResult<TElement>> ExecuteAsync<TElement>(Func<QueryEndpoint, IOrderedQueryable<TElement>> query) =>
      new CompiledQueryRunner(this, query.Method, query.Target).ExecuteCompiledAsync(query, CancellationToken.None);

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="query"/> delegate
    /// (in fact, by its <see cref="MethodInfo"/> instance)
    /// and asynchronously executes them if it's already cached;
    /// otherwise asynchronously executes the <paramref name="query"/> delegate
    /// and caches the result.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <param name="token">Token to cancel operation.</param>
    /// <returns>Task performing operation.</returns>
    public Task<QueryResult<TElement>> ExecuteAsync<TElement>(Func<QueryEndpoint, IOrderedQueryable<TElement>> query, CancellationToken token) =>
      new CompiledQueryRunner(this, query.Method, query.Target).ExecuteCompiledAsync(query, token);

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="key"/>
    /// and asynchronously executes them if it's already cached;
    /// otherwise asynchronously executes the <paramref name="query"/> delegate
    /// and caches the result.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="key">An object identifying this query in cache.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>Task performing operation.</returns>
    public Task<QueryResult<TElement>> ExecuteAsync<TElement>(object key, Func<QueryEndpoint, IOrderedQueryable<TElement>> query) =>
      new CompiledQueryRunner(this, key, query.Target).ExecuteCompiledAsync(query, CancellationToken.None);

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="key"/>
    /// and asynchronously executes them if it's already cached;
    /// otherwise asynchronously executes the <paramref name="query"/> delegate
    /// and caches the result.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="key">An object identifying this query in cache.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <param name="token">Token to cancel operation.</param>
    /// <returns>Task performing operation.</returns>
    public Task<QueryResult<TElement>> ExecuteAsync<TElement>(object key, Func<QueryEndpoint, IOrderedQueryable<TElement>> query, CancellationToken token) =>
      new CompiledQueryRunner(this, key, query.Target).ExecuteCompiledAsync(query, token);

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="query"/> delegate
    /// (in fact, by its <see cref="MethodInfo"/> instance)
    /// and asynchronously executes them if it's already cached;
    /// otherwise asynchronously executes the <paramref name="query"/> delegate
    /// and caches the result.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>Task performing operation.</returns>
    public Task<TResult> ExecuteAsync<TResult>(Func<QueryEndpoint, TResult> query) =>
      new CompiledQueryRunner(this, query.Method, query.Target).ExecuteCompiledAsync(query, CancellationToken.None);

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="query"/> delegate
    /// (in fact, by its <see cref="MethodInfo"/> instance)
    /// and asynchronously executes them if it's already cached;
    /// otherwise asynchronously executes the <paramref name="query"/> delegate
    /// and caches the result.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <param name="token">Token to cancel operation.</param>
    /// <returns>Task performing operation.</returns>
    public Task<TResult> ExecuteAsync<TResult>(Func<QueryEndpoint, TResult> query, CancellationToken token) =>
      new CompiledQueryRunner(this, query.Method, query.Target).ExecuteCompiledAsync(query, token);

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="key"/>
    /// and asynchronously executes them if it's already cached;
    /// otherwise asynchronously executes the <paramref name="query"/> delegate
    /// and caches the result.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="key">An object identifying this query in cache.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>Task performing operation.</returns>
    public Task<TResult> ExecuteAsync<TResult>(object key, Func<QueryEndpoint, TResult> query) =>
      new CompiledQueryRunner(this, key, query.Target).ExecuteCompiledAsync(query, CancellationToken.None);

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="key"/>
    /// and asynchronously executes them if it's already cached;
    /// otherwise asynchronously executes the <paramref name="query"/> delegate
    /// and caches the result.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="key">An object identifying this query in cache.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <param name="token">Token to cancel operation.</param>
    /// <returns>Task performing operation.</returns>
    public Task<TResult> ExecuteAsync<TResult>(object key, Func<QueryEndpoint, TResult> query, CancellationToken token) =>
      new CompiledQueryRunner(this, key, query.Target).ExecuteCompiledAsync(query, token);

    #endregion

    #region Delayed Queries

    /// <summary>
    /// Creates future scalar query and registers it for the later execution.
    /// The query associated with the future scalar will be cached.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="key">An object identifying this query in cache.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>
    /// The future that will be executed when its result is requested.
    /// </returns>
    public DelayedScalarQuery<TResult> CreateDelayedQuery<TResult>(object key, Func<QueryEndpoint, TResult> query) =>
      new CompiledQueryRunner(this, key, query.Target).CreateDelayedQuery(query);

    /// <summary>
    /// Creates future scalar query and registers it for the later execution.
    /// The query associated with the future scalar will not be cached.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>
    /// The future that will be executed when its result is requested.
    /// </returns>
    public DelayedScalarQuery<TResult> CreateDelayedQuery<TResult>(Func<QueryEndpoint, TResult> query) =>
      new CompiledQueryRunner(this, query.Method, query.Target).CreateDelayedQuery(query);

    /// <summary>
    /// Creates future query and registers it for the later execution.
    /// The associated query will be cached.
    /// </summary>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="key">An object identifying this query in cache.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>
    /// The future that will be executed when its result is requested.
    /// </returns>
    public DelayedQuery<TElement> CreateDelayedQuery<TElement>(object key, Func<QueryEndpoint, IQueryable<TElement>> query) =>
      new CompiledQueryRunner(this, key, query.Target).CreateDelayedQuery(query);

    /// <summary>
    /// Creates future query and registers it for the later execution.
    /// The associated query will be cached.
    /// </summary>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>
    /// The future that will be executed when its result is requested.
    /// </returns>
    public DelayedQuery<TElement> CreateDelayedQuery<TElement>(Func<QueryEndpoint, IQueryable<TElement>> query) =>
      new CompiledQueryRunner(this, query.Method, query.Target).CreateDelayedQuery(query);

    /// <summary>
    /// Creates future query and registers it for the later execution.
    /// The associated query will be cached.
    /// </summary>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="key">An object identifying this query in cache.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>
    /// The future that will be executed when its result is requested.
    /// </returns>
    public DelayedQuery<TElement> CreateDelayedQuery<TElement>(object key, Func<QueryEndpoint, IOrderedQueryable<TElement>> query) =>
      new CompiledQueryRunner(this, key, query.Target).CreateDelayedQuery(query);

    /// <summary>
    /// Creates future query and registers it for the later execution.
    /// The associated query will be cached.
    /// </summary>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>
    /// The future that will be executed when its result is requested.
    /// </returns>
    public DelayedQuery<TElement> CreateDelayedQuery<TElement>(Func<QueryEndpoint, IOrderedQueryable<TElement>> query) =>
      new CompiledQueryRunner(this, query.Method, query.Target).CreateDelayedQuery(query);

    /// <summary>
    /// Creates future scalar query and registers it for the later execution.
    /// The query associated with the future scalar will be cached.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="key">An object identifying this query in cache.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>
    /// The future that will be executed when its result is requested.
    /// </returns>
    [Obsolete("ExecuteDelayed method is obsolete. Use corresponding CreateDelayedQuery method instead")]
    public DelayedScalarQuery<TResult> ExecuteDelayed<TResult>(object key, Func<QueryEndpoint, TResult> query) =>
      CreateDelayedQuery(key, query);

    /// <summary>
    /// Creates future scalar query and registers it for the later execution.
    /// The query associated with the future scalar will not be cached.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>
    /// The future that will be executed when its result is requested.
    /// </returns>
    [Obsolete("ExecuteDelayed method is obsolete. Use corresponding CreateDelayedQuery method instead")]
    public DelayedScalarQuery<TResult> ExecuteDelayed<TResult>(Func<QueryEndpoint, TResult> query) =>
      CreateDelayedQuery(query);

    /// <summary>
    /// Creates future query and registers it for the later execution.
    /// The associated query will be cached.
    /// </summary>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="key">An object identifying this query in cache.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>
    /// The future that will be executed when its result is requested.
    /// </returns>
    [Obsolete("ExecuteDelayed method is obsolete. Use corresponding CreateDelayedQuery method instead")]
    public IEnumerable<TElement> ExecuteDelayed<TElement>(object key, Func<QueryEndpoint, IQueryable<TElement>> query) =>
      CreateDelayedQuery(key, query);

    /// <summary>
    /// Creates future query and registers it for the later execution.
    /// The associated query will be cached.
    /// </summary>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>
    /// The future that will be executed when its result is requested.
    /// </returns>
    [Obsolete("ExecuteDelayed method is obsolete. Use corresponding CreateDelayedQuery method instead")]
    public IEnumerable<TElement> ExecuteDelayed<TElement>(Func<QueryEndpoint, IQueryable<TElement>> query) =>
      CreateDelayedQuery(query);

    /// <summary>
    /// Creates future query and registers it for the later execution.
    /// The associated query will be cached.
    /// </summary>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="key">An object identifying this query in cache.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>
    /// The future that will be executed when its result is requested.
    /// </returns>
    [Obsolete("ExecuteDelayed method is obsolete. Use corresponding CreateDelayedQuery method instead")]
    public IEnumerable<TElement> ExecuteDelayed<TElement>(object key, Func<QueryEndpoint, IOrderedQueryable<TElement>> query) =>
      CreateDelayedQuery(key, query);

    /// <summary>
    /// Creates future query and registers it for the later execution.
    /// The associated query will be cached.
    /// </summary>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>
    /// The future that will be executed when its result is requested.
    /// </returns>
    [Obsolete("ExecuteDelayed method is obsolete. Use corresponding CreateDelayedQuery method instead")]
    public IEnumerable<TElement> ExecuteDelayed<TElement>(Func<QueryEndpoint, IOrderedQueryable<TElement>> query) =>
      CreateDelayedQuery(query);

    #endregion

    /// <summary>
    /// Stores specified <paramref name="source"/> in the database
    /// and provides a query for stored items.
    /// </summary>
    /// <typeparam name="TElement">Item type.</typeparam>
    /// <param name="source">Items to store.</param>
    /// <returns>Query for stored items.</returns>
    public IQueryable<TElement> Store<TElement>(IEnumerable<TElement> source)
    {
      var method = WellKnownMembers.Queryable.AsQueryable.CachedMakeGenericMethod(typeof(TElement));
      var expression = Expression.Call(method, Expression.Constant(source));
      return Provider.CreateQuery<TElement>(expression);
    }

    /// <summary>
    /// Creates query for the <see cref="EntitySet{TItem}"/>
    /// defined by <paramref name="provider"/> expression.
    /// This method is suitable for usage within compiled queries.
    /// For regular queries you can query <see cref="EntitySet{TItem}"/> directly.
    /// </summary>
    /// <typeparam name="TElement">Entity set element type.</typeparam>
    /// <param name="provider">Expression that defines <see cref="EntitySet{TItem}"/>.</param>
    /// <returns>Created query.</returns>
    public IQueryable<TElement> Items<TElement>(Expression<Func<EntitySet<TElement>>> provider)
      where TElement : IEntity
    {
      return Provider.CreateQuery<TElement>(provider.Body);
    }

    #region Private / internal methods

    /// <exception cref="ArgumentException"><paramref name="keyValues"/> array is empty.</exception>
    private Key GetKeyByValues<T>(object[] keyValues)
      where T : class, IEntity
    {
      ArgumentValidator.EnsureArgumentNotNull(keyValues, "keyValues");
      if (keyValues.Length == 0)
        throw new ArgumentException(Strings.ExKeyValuesArrayIsEmpty, "keyValues");
      if (keyValues.Length == 1) {
        switch (keyValues[0]) {
          case Key key:
            return key;
          case Entity entity:
            return entity.Key;
        }
      }
      return Key.Create(session.Domain, session.StorageNodeId, typeof(T), TypeReferenceAccuracy.BaseType, keyValues);
    }

    private Expression BuildRootExpression(Type elementType)
    {
      return RootBuilder!=null
        ? RootBuilder.BuildRootExpression(elementType)
        : Expression.Call(null, WellKnownMembers.Query.All.CachedMakeGenericMethod(elementType));
    }

    #endregion


    // Constructors

    internal QueryEndpoint(QueryProvider provider)
    {
      ArgumentValidator.EnsureArgumentNotNull(provider, "provider");
      Provider = provider;
      session = provider.Session;
    }

    internal QueryEndpoint(QueryEndpoint outerEndpoint, IQueryRootBuilder queryRootBuilder)
    {
      ArgumentValidator.EnsureArgumentNotNull(outerEndpoint, "outerEndpoint");
      ArgumentValidator.EnsureArgumentNotNull(queryRootBuilder, "queryRootBuilder");
      Provider = outerEndpoint.Provider;
      session = outerEndpoint.session;
      RootBuilder = queryRootBuilder;
    }
  }
}
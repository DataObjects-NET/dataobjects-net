// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.07.27

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Xtensive.Orm.FullTextSearchCondition.Interfaces;
using Xtensive.Orm.FullTextSearchCondition.Nodes;
using Xtensive.Orm.Internals;

namespace Xtensive.Orm
{
  /// <summary>
  /// Single access point allowing to run LINQ queries,
  /// create future (delayed) and compiled queries,
  /// and finally, resolve <see cref="Key"/>s to <see cref="Entity">entities</see>.
  /// </summary>
  [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
  public static class Query
  {
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
    public static IQueryable<T> All<T>()
      where T: class, IEntity
    {
      return Session.Demand().Query.All<T>();
    }

    public static IQueryable<T> AllNew<T>(string callerMemberName)
      where T: class, IEntity
    {
      return Session.Demand().Query.All<T>();
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
    public static IQueryable All(Type elementType)
    {
      return Session.Demand().Query.All(elementType);
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
    public static IQueryable<FullTextMatch<T>> FreeText<T>(string searchCriteria)
      where T: Entity
    {
      return Session.Demand().Query.FreeText<T>(searchCriteria);
    }

    /// <summary>
    /// Performs full-text query for the text specified in free text form.
    /// Limits the result by top number of elements, sorted by rank in descending order.
    /// </summary>
    /// <typeparam name="T">Type of the entity to query full-text index of.</typeparam>
    /// <param name="searchCriteria">The search criteria in free text form.</param>
    /// <param name="topNByRank">Limits the query result by topN elements.</param>
    /// <returns></returns>
    public static IQueryable<FullTextMatch<T>> FreeText<T>(string searchCriteria, int topNByRank)
      where T: Entity
    {
      return Session.Demand().Query.FreeText<T>(searchCriteria, topNByRank);
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
    public static IQueryable<FullTextMatch<T>> FreeText<T>(Expression<Func<string>> searchCriteria)
      where T: Entity
    {
      return Session.Demand().Query.FreeText<T>(searchCriteria);
    }

    /// <summary>
    /// Performs full-text query for the text specified in free text form.
    /// Limits the result by top number of elements, sorted by rank in descending order.
    /// </summary>
    /// <typeparam name="T">Type of the entity to query full-text index of.</typeparam>
    /// <param name="searchCriteria">The search criteria in free text form.</param>
    /// <param name="topNByRank">Limits the query resutlt by topN elements.</param>
    /// <returns></returns>
    public static IQueryable<FullTextMatch<T>> FreeText<T>(Expression<Func<string>> searchCriteria, int topNByRank) 
      where T : Entity
    {
       return Session.Demand().Query.FreeText<T>(searchCriteria, topNByRank);
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
    public static IQueryable<FullTextMatch<T>> ContainsTable<T>([NotNull] Expression<Func<ConditionEndpoint, IOperand>> searchCriteria)
      where T : Entity
    {
      return Session.Demand().Query.ContainsTable<T>(searchCriteria);
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
    public static IQueryable<FullTextMatch<T>> ContainsTable<T>(
      [NotNull] Expression<Func<ConditionEndpoint, IOperand>> searchCriteria,
      [NotNull] Expression<Func<T, object>>[] targetFields)
      where T : Entity
    {
      return Session.Demand().Query.ContainsTable<T>(searchCriteria, targetFields);
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
    public static IQueryable<FullTextMatch<T>> ContainsTable<T>([NotNull]Expression<Func<ConditionEndpoint, IOperand>> searchCriteria, int topNByRank)
      where T : Entity
    {
      return Session.Demand().Query.ContainsTable<T>(searchCriteria, topNByRank);
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
    public static IQueryable<FullTextMatch<T>> ContainsTable<T>(
      [NotNull] Expression<Func<ConditionEndpoint, IOperand>> searchCriteria,
      [NotNull] Expression<Func<T, object>>[] targetFields,
      int topNByRank)
      where T : Entity
    {
      return Session.Demand().Query.ContainsTable<T>(searchCriteria, targetFields, topNByRank);
    }

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
      return Session.Demand().Query.Single(key);
    }

    /// <summary>
    /// Resolves (gets) the <see cref="Entity"/> by the specified <paramref name="key"/>
    /// in the current <see cref="Session"/>.
    /// </summary>
    /// <param name="key">The key to resolve.</param>
    /// <returns>
    /// The <see cref="Entity"/> specified <paramref name="key"/> identifies.
    /// <see langword="null"/>, if there is no such entity.
    /// </returns>
    [CanBeNull] public static Entity SingleOrDefault(Key key)
    {
      return Session.Demand().Query.SingleOrDefault(key);
    }

    /// <summary>
    /// Resolves (gets) the <see cref="Entity"/> by the specified <paramref name="key"/>
    /// in the current <see cref="Session"/>.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="key">The key to resolve.</param>
    /// <returns>
    /// The <see cref="Entity"/> specified <paramref name="key"/> identifies.
    /// <see langword="null"/>, if there is no such entity.
    /// </returns>
    public static T Single<T>(Key key)
      where T : class, IEntity
    {
      return Session.Demand().Query.Single<T>(key);
    }

    /// <summary>
    /// Resolves (gets) the <see cref="Entity"/> by the specified <paramref name="keyValues"/>
    /// in the current <see cref="Session"/>.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="keyValues">Key values.</param>
    /// <returns>
    /// The <see cref="Entity"/> specified <paramref name="keyValues"/> identify.
    /// <see langword="null"/>, if there is no such entity.
    /// </returns>
    public static T Single<T>(params object[] keyValues)
      where T : class, IEntity
    {
      return Session.Demand().Query.Single<T>(keyValues);
    }

    /// <summary>
    /// Resolves (gets) the <see cref="Entity"/> by the specified <paramref name="key"/>
    /// in the current <see cref="Session"/>.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="key">The key to resolve.</param>
    /// <returns>
    /// The <see cref="Entity"/> specified <paramref name="key"/> identifies.
    /// </returns>
    [CanBeNull] public static T SingleOrDefault<T>(Key key)
      where T : class, IEntity
    {
      return Session.Demand().Query.SingleOrDefault<T>(key);
    }

    /// <summary>
    /// Resolves (gets) the <see cref="Entity"/> by the specified <paramref name="keyValues"/>
    /// in the current <see cref="Session"/>.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <param name="keyValues">Key values.</param>
    /// <returns>
    /// The <see cref="Entity"/> specified <paramref name="keyValues"/> identify.
    /// </returns>
    [CanBeNull] public static T SingleOrDefault<T>(params object[] keyValues)
      where T : class, IEntity
    {
      return Session.Demand().Query.SingleOrDefault<T>(keyValues);
    }

    #region Execute

    /// <summary>
    /// Finds compiled query in cache by specified <paramref name="query"/> delegate
    /// (in fact, by its <see cref="MethodInfo"/> instance)
    /// and executes it, if found;
    /// otherwise executes the <paramref name="query"/> delegate
    /// and caches the compilation result.
    /// </summary>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>Query result.</returns>
    public static IEnumerable<TElement> Execute<TElement>(Func<IQueryable<TElement>> query)
    {
      var endpoint = Session.Demand().Query;
      return new CompiledQueryRunner(endpoint, query.Method, query.Target).ExecuteCompiled(WrapQuery(query));
    }

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="key"/>
    /// and executes it, if found;
    /// otherwise executes the <paramref name="query"/> delegate
    /// and caches the compilation result.
    /// </summary>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="key">An object identifying this query in cache.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>Query result.</returns>
    public static IEnumerable<TElement> Execute<TElement>(object key, Func<IQueryable<TElement>> query)
    {
      var endpoint = Session.Demand().Query;
      return new CompiledQueryRunner(endpoint, key, query.Target).ExecuteCompiled(WrapQuery(query));
    }

    /// <summary>
    /// Finds compiled query in cache by specified <paramref name="query"/> delegate
    /// (in fact, by its <see cref="MethodInfo"/> instance)
    /// and executes it, if found;
    /// otherwise executes the <paramref name="query"/> delegate
    /// and caches the compilation result.
    /// </summary>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>Query result.</returns>
    public static IEnumerable<TElement> Execute<TElement>(Func<IOrderedQueryable<TElement>> query)
    {
      var endpoint = Session.Demand().Query;
      return new CompiledQueryRunner(endpoint, query.Method, query.Target).ExecuteCompiled(WrapQuery(query));
    }

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="key"/>
    /// and executes it, if found;
    /// otherwise executes the <paramref name="query"/> delegate
    /// and caches the compilation result.
    /// </summary>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="key">An object identifying this query in cache.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>Query result.</returns>
    public static IEnumerable<TElement> Execute<TElement>(object key, Func<IOrderedQueryable<TElement>> query)
    {
      var endpoint = Session.Demand().Query;
      return new CompiledQueryRunner(endpoint, key, query.Target).ExecuteCompiled(WrapQuery(query));
    }

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="query"/> delegate
    /// (in fact, by its <see cref="MethodInfo"/> instance)
    /// and executes it, if found;
    /// otherwise executes the <paramref name="query"/> delegate
    /// and caches the compilation result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>Query result.</returns>
    public static TResult Execute<TResult>(Func<TResult> query)
    {
      var endpoint = Session.Demand().Query;
      return new CompiledQueryRunner(endpoint, query.Method, query.Target).ExecuteCompiled(WrapQuery(query));
    }

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="key"/>
    /// and executes it, if found;
    /// otherwise executes the <paramref name="query"/> delegate
    /// and caches the compilation result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="key">An object identifying this query in cache.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>Query result.</returns>
    public static TResult Execute<TResult>(object key, Func<TResult> query)
    {
      var endpoint = Session.Demand().Query;
      return new CompiledQueryRunner(endpoint, key, query.Target).ExecuteCompiled(WrapQuery(query));
    }

    #region Async

    /// <summary>
    /// Finds compiled query in cache by specified <paramref name="query"/> delegate
    /// (in fact, by its <see cref="MethodInfo"/> instance)
    /// and asynchronously executes it, if found;
    /// otherwise asynchronously executes the <paramref name="query"/> delegate
    /// and caches the compilation result.
    /// </summary>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>Task performing this operation.</returns>
    public static Task<IEnumerable<TElement>> ExecuteAsync<TElement>(Func<IQueryable<TElement>> query)
    {
      return ExecuteAsync<TElement>(query, CancellationToken.None);
    }

    /// <summary>
    /// Finds compiled query in cache by specified <paramref name="query"/> delegate
    /// (in fact, by its <see cref="MethodInfo"/> instance)
    /// and asynchronously executes it, if found;
    /// otherwise asynchronously executes the <paramref name="query"/> delegate
    /// and caches the compilation result.
    /// </summary>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <param name="token">A token to cancel operation.</param>
    /// <returns>Task performing this operation.</returns>
    public static Task<IEnumerable<TElement>> ExecuteAsync<TElement>(Func<IQueryable<TElement>> query, CancellationToken token)
    {
      var endpoint = Session.Demand().Query;
      token.ThrowIfCancellationRequested();
      return new CompiledQueryRunner(endpoint, query.Method, query.Target).ExecuteCompiledAsync(WrapQuery(query), token);
    }

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="key"/>
    /// and asynchronously executes it, if found;
    /// otherwise asynchronously executes the <paramref name="query"/> delegate
    /// and caches the compilation result.
    /// </summary>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="key">An object identifying this query in cache.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>Task preforming this operation.</returns>
    public static Task<IEnumerable<TElement>> ExecuteAsync<TElement>(object key, Func<IQueryable<TElement>> query)
    {
      return ExecuteAsync(key, query, CancellationToken.None);
    }

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="key"/>
    /// and asynchronously executes it, if found;
    /// otherwise asynchronously executes the <paramref name="query"/> delegate
    /// and caches the compilation result.
    /// </summary>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="key">An object identifying this query in cache.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <param name="token">A token to cancel operation.</param>
    /// <returns>Task preforming this operation.</returns>
    public static Task<IEnumerable<TElement>> ExecuteAsync<TElement>(object key, Func<IQueryable<TElement>> query, CancellationToken token)
    {
      var endpoint = Session.Demand().Query;
      token.ThrowIfCancellationRequested();
      return new CompiledQueryRunner(endpoint, key, query.Target).ExecuteCompiledAsync(WrapQuery(query), token);
    }

    /// <summary>
    /// Finds compiled query in cache by specified <paramref name="query"/> delegate
    /// (in fact, by its <see cref="MethodInfo"/> instance)
    /// and asynchronously executes it, if found;
    /// otherwise asynchronously executes the <paramref name="query"/> delegate
    /// and caches the compilation result.
    /// </summary>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>Task preforming this operation.</returns>
    public static Task<IEnumerable<TElement>> ExecuteAsync<TElement>(Func<IOrderedQueryable<TElement>> query)
    {
      return ExecuteAsync(query, CancellationToken.None);
    }

    /// <summary>
    /// Finds compiled query in cache by specified <paramref name="query"/> delegate
    /// (in fact, by its <see cref="MethodInfo"/> instance)
    /// and asynchronously executes it, if found;
    /// otherwise executes the <paramref name="query"/> delegate
    /// and caches the compilation result.
    /// </summary>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <param name="token">A token to cancel operation.</param>
    /// <returns>Task preforming this operation.</returns>
    public static Task<IEnumerable<TElement>> ExecuteAsync<TElement>(Func<IOrderedQueryable<TElement>> query, CancellationToken token)
    {
      var endpoint = Session.Demand().Query;
      token.ThrowIfCancellationRequested();
      return new CompiledQueryRunner(endpoint, query.Method, query.Target).ExecuteCompiledAsync(WrapQuery(query), token);
    }

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="key"/>
    /// and asynchronously executes it, if found;
    /// otherwise executes the <paramref name="query"/> delegate
    /// and caches the compilation result.
    /// </summary>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="key">An object identifying this query in cache.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>Task preforming this operation.</returns>
    public static Task<IEnumerable<TElement>> ExecuteAsync<TElement>(object key, Func<IOrderedQueryable<TElement>> query)
    {
      return ExecuteAsync(key, query, CancellationToken.None);
    }

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="key"/>
    /// and asynchronously executes it, if found;
    /// otherwise executes the <paramref name="query"/> delegate
    /// and caches the compilation result.
    /// </summary>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="key">An object identifying this query in cache.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <param name="token">A token to cancel operation.</param>
    /// <returns>Task preforming this operation.</returns>
    public static Task<IEnumerable<TElement>> ExecuteAsync<TElement>(object key, Func<IOrderedQueryable<TElement>> query, CancellationToken token)
    {
      var endpoint = Session.Demand().Query;
      token.ThrowIfCancellationRequested();
      return new CompiledQueryRunner(endpoint, key, query.Target).ExecuteCompiledAsync(WrapQuery(query), token);
    }

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="query"/> delegate
    /// (in fact, by its <see cref="MethodInfo"/> instance)
    /// and asynchronously executes it, if found;
    /// otherwise asynchronously executes the <paramref name="query"/> delegate
    /// and caches the compilation result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>Task preforming this operation.</returns>
    public static Task<TResult> ExecuteAsync<TResult>(Func<TResult> query)
    {
      return ExecuteAsync(query, CancellationToken.None);
    }

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="query"/> delegate
    /// (in fact, by its <see cref="MethodInfo"/> instance)
    /// and asynchronously executes it, if found;
    /// otherwise asynchronously executes the <paramref name="query"/> delegate
    /// and caches the compilation result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <param name="token">A token to cancel operation.</param>
    /// <returns>Task preforming this operation.</returns>
    public static Task<TResult> ExecuteAsync<TResult>(Func<TResult> query, CancellationToken token)
    {
      var endpoint = Session.Demand().Query;
      token.ThrowIfCancellationRequested();
      return new CompiledQueryRunner(endpoint, query.Method, query.Target).ExecuteCompiledAsync(WrapQuery(query), token);
    }

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="key"/>
    /// and asynchronously executes it, if found;
    /// otherwise asynchronously executes the <paramref name="query"/> delegate
    /// and caches the compilation result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="key">An object identifying this query in cache.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>Task preforming this operation.</returns>
    public static Task<TResult> ExecuteAsync<TResult>(object key, Func<TResult> query)
    {
      return ExecuteAsync(key, query, CancellationToken.None);
    }

    /// <summary>
    /// Finds compiled query in cache by provided <paramref name="key"/>
    /// and asynchronously executes it, if found;
    /// otherwise asynchronously executes the <paramref name="query"/> delegate
    /// and caches the compilation result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="key">An object identifying this query in cache.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <param name="token">A token to cancel operation.</param>
    /// <returns>Task preforming this operation.</returns>
    public static Task<TResult> ExecuteAsync<TResult>(object key, Func<TResult> query, CancellationToken token)
    {
      var endpoint = Session.Demand().Query;
      token.ThrowIfCancellationRequested();
      return new CompiledQueryRunner(endpoint, key, query.Target).ExecuteCompiledAsync(WrapQuery(query), token);
    }

    #endregion

    #endregion

    #region Future methods

    /// <summary>
    /// Creates future scalar query and registers it for the later execution.
    /// The query compilation result associated with the future scalar will be cached as well.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="key">An object identifying this query in cache.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>
    /// The future that will be executed when its result is requested.
    /// </returns>
    public static Delayed<TResult> ExecuteFutureScalar<TResult>(object key, Func<TResult> query)
    {
      var endpoint = Session.Demand().Query;
      return new CompiledQueryRunner(endpoint, key, query.Target).ExecuteDelayed(WrapQuery(query));
    }

    /// <summary>
    /// Creates future scalar query and registers it for the later execution.
    /// The query compilation result associated with the future scalar will be cached as well.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>
    /// The future that will be executed when its result is requested.
    /// </returns>
    public static Delayed<TResult> ExecuteFutureScalar<TResult>(Func<TResult> query)
    {
      var endpoint = Session.Demand().Query;
      return new CompiledQueryRunner(endpoint, query.Method, query.Target).ExecuteDelayed(WrapQuery(query));
    }

    /// <summary>
    /// Creates future query and registers it for the later execution.
    /// The query compilation result will be cached as well.
    /// </summary>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="key">An object identifying this query in cache.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>
    /// The future that will be executed when its result is requested.
    /// </returns>
    public static IEnumerable<TElement> ExecuteFuture<TElement>(object key, Func<IQueryable<TElement>> query)
    {
      var endpoint = Session.Demand().Query;
      return new CompiledQueryRunner(endpoint, key, query.Target).ExecuteDelayed(WrapQuery(query));
    }

    /// <summary>
    /// Creates future query and registers it for the later execution.
    /// The query compilation result will be cached as well.
    /// </summary>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>
    /// The future that will be executed when its result is requested.
    /// </returns>
    public static IEnumerable<TElement> ExecuteFuture<TElement>(Func<IQueryable<TElement>> query)
    {
      var endpoint = Session.Demand().Query;
      return new CompiledQueryRunner(endpoint, query.Method, query.Target).ExecuteDelayed(WrapQuery(query));
    }

    /// <summary>
    /// Creates future query and registers it for the later execution.
    /// The query compilation result will be cached as well.
    /// </summary>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>
    /// The future that will be executed when its result is requested.
    /// </returns>
    public static IEnumerable<TElement> ExecuteFuture<TElement>(Func<IOrderedQueryable<TElement>> query)
    {
      var endpoint = Session.Demand().Query;
      return new CompiledQueryRunner(endpoint, query.Method, query.Target).ExecuteDelayed(WrapQuery(query));
    }

    /// <summary>
    /// Creates future query and registers it for the later execution.
    /// The query compilation result will be cached as well.
    /// </summary>
    /// <typeparam name="TElement">The type of the resulting sequence element.</typeparam>
    /// <param name="key">An object identifying this query in cache.</param>
    /// <param name="query">A delegate performing the query to cache.</param>
    /// <returns>
    /// The future that will be executed when its result is requested.
    /// </returns>
    public static IEnumerable<TElement> ExecuteFuture<TElement>(object key, Func<IOrderedQueryable<TElement>> query)
    {
      var endpoint = Session.Demand().Query;
      return new CompiledQueryRunner(endpoint, query.Method, query.Target).ExecuteDelayed(WrapQuery(query));
    }

    #endregion

    private static Func<QueryEndpoint, TResult> WrapQuery<TResult>(Func<TResult> query)
    {
      return _ => query.Invoke();
    }

    /// <summary>
    /// Stores the specified sequence of keys (<see cref="Entity">entities</see>),
    /// anonymous types or DTOs to the database and allows it to use as
    /// <see cref="IQueryable{T}"/> further.
    /// </summary>
    /// <typeparam name="TElement">The type of the sequence element.</typeparam>
    /// <param name="source">The sequence to store.</param>
    /// <returns><see cref="IQueryable{T}"/> providing access to the stored sequence.</returns>
    public static IQueryable<TElement> Store<TElement>(IEnumerable<TElement> source)
    {
      return Session.Demand().Query.Store(source);
    }
  }
}
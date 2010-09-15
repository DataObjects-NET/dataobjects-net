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

namespace Xtensive.Storage
{
  /// <summary>
  /// Single access point allowing to run LINQ queries,
  /// create future (delayed) and compiled queries,
  /// and finally, resolve <see cref="Key"/>s to <see cref="Entity">entities</see>.
  /// </summary>
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
    public static Entity SingleOrDefault(Key key)
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
    public static T SingleOrDefault<T>(Key key)
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
    public static T SingleOrDefault<T>(params object[] keyValues)
      where T : class, IEntity
    {
      return Session.Demand().Query.SingleOrDefault<T>(keyValues);
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
    public static IEnumerable<TElement> Execute<TElement>(Func<IQueryable<TElement>> query)
    {
      return Session.Demand().Query.Execute(query.Method, query);
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
      return Session.Demand().Query.Execute(key, query);
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
      return Session.Demand().Query.Execute(query);
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
      return Session.Demand().Query.Execute(key, query);
    }

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
    public static FutureScalar<TResult> ExecuteFutureScalar<TResult>(object key, Func<TResult> query)
    {
      return Session.Demand().Query.ExecuteFutureScalar(key, query);
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
    public static FutureScalar<TResult> ExecuteFutureScalar<TResult>(Func<TResult> query)
    {
      return Session.Demand().Query.ExecuteFutureScalar(query);
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
      return Session.Demand().Query.ExecuteFuture(key, query);
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
      return Session.Demand().Query.ExecuteFuture(query);
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
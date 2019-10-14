// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.05.06

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Xtensive.Core;
using Xtensive.Orm.Linq;

namespace Xtensive.Orm
{
  /// <summary>
  /// Extends LINQ methods for <see cref="Xtensive.Orm.Linq"/> queries. 
  /// </summary>
  public static class QueryableExtensions
  {
    /// <summary>
    /// Returns the number of elements in <paramref name="source"/> sequence.
    /// </summary>
    /// <param name="source">The source sequence.</param>
    public static int Count([InstantHandle] this IQueryable source)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      return (int) source.Provider.Execute(
        Expression.Call(
          typeof (Queryable), "Count",
          new[] {source.ElementType}, source.Expression));
    }

    /// <summary>
    /// Version of <see cref="Queryable.Take{TSource}"/>, where <paramref name="count"/> is specified as 
    /// <see cref="Expression"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of the source element.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="count">The count of items to take.</param>
    /// <returns>The same result as its original version.</returns>
    public static IQueryable<TSource> Take<TSource>(this IQueryable<TSource> source, Expression<Func<int>> count)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(count, "count");

      var errorMessage = Strings.ExTakeDoesNotSupportQueryProviderOfTypeX;
      var providerType = source.Provider.GetType();
      if (providerType!=typeof (QueryProvider))
        throw new NotSupportedException(String.Format(errorMessage, providerType));

      var genericMethod = WellKnownMembers.Queryable.ExtensionTake.MakeGenericMethod(new[] {typeof (TSource)});
      var expression = Expression.Call(null, genericMethod, new[] {source.Expression, count});
      return source.Provider.CreateQuery<TSource>(expression);
    }

    /// <summary>
    /// Version of <see cref="Queryable.Skip{TSource}"/>, where <paramref name="count"/> is specified as 
    /// <see cref="Expression"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of the source element.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="count">The count of items to skip.</param>
    /// <returns>The same result as its original version.</returns>
    public static IQueryable<TSource> Skip<TSource>(this IQueryable<TSource> source, Expression<Func<int>> count)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(count, "count");

      var errorMessage = Strings.ExSkipDoesNotSupportQueryProviderOfTypeX;
      var providerType = source.Provider.GetType();
      if (providerType!=typeof (QueryProvider))
        throw new NotSupportedException(String.Format(errorMessage, providerType));

      var genericMethod = WellKnownMembers.Queryable.ExtensionSkip.MakeGenericMethod(new[] {typeof (TSource)});
      var expression = Expression.Call(null, genericMethod, new[] {source.Expression, count});
      return source.Provider.CreateQuery<TSource>(expression);
    }

    /// <summary>
    /// Version of <see cref="Queryable.ElementAt{TSource}"/>, where <paramref name="index"/> is specified as
    /// <see cref="Expression"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of the source element.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="index">The index of element to take.</param>
    /// <returns>The same result as its original version.</returns>
    public static TSource ElementAt<TSource>(this IQueryable<TSource> source, Expression<Func<int>> index)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(index, "index");

      var errorMessage = Strings.ExElementAtDoesNotSupportQueryProviderOfTypeX;
      var providerType = source.Provider.GetType();
      if (providerType!=typeof (QueryProvider))
        throw new NotSupportedException(String.Format(errorMessage, providerType));

      var genericMethod = WellKnownMembers.Queryable.ExtensionElementAt.MakeGenericMethod(new[] {typeof (TSource)});
      var expression = Expression.Call(null, genericMethod, new[] {source.Expression, index});
      return source.Provider.Execute<TSource>(expression);
    }

    /// <summary>
    /// Version of <see cref="Queryable.ElementAtOrDefault{TSource}"/>, where <paramref name="index"/> is specified as
    /// <see cref="Expression"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of the source element.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="index">The index of element to take.</param>
    /// <returns>The same result as its original version.</returns>
    public static TSource ElementAtOrDefault<TSource>(this IQueryable<TSource> source, Expression<Func<int>> index)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(index, "index");

      var errorMessage = Strings.ExElementAtOrDefaultDoesNotSupportQueryProviderOfTypeX;
      var providerType = source.Provider.GetType();
      if (providerType!=typeof (QueryProvider))
        throw new NotSupportedException(String.Format(errorMessage, providerType));

      var genericMethod = WellKnownMembers.Queryable.ExtensionElementAtOrDefault.MakeGenericMethod(new[] {typeof (TSource)});
      var expression = Expression.Call(null, genericMethod, new[] {source.Expression, index});
      return source.Provider.Execute<TSource>(expression);
    }

    /// <summary>
    /// Applies locks to the specified source queryable.
    /// </summary>
    /// <typeparam name="TSource">The type of the source element.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="lockMode">The lock mode.</param>
    /// <param name="lockBehavior">The lock behavior.</param>
    /// <returns>The same sequence, but with "apply lock" hint.</returns>
    public static IQueryable<TSource> Lock<TSource>(this IQueryable<TSource> source, LockMode lockMode, LockBehavior lockBehavior)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(lockMode, "lockMode");
      ArgumentValidator.EnsureArgumentNotNull(lockBehavior, "lockBehavior");
      var errorMessage = Strings.ExLockDoesNotSupportQueryProviderOfTypeX;
      var providerType = source.Provider.GetType();
      if (providerType!=typeof (QueryProvider))
        throw new NotSupportedException(String.Format(errorMessage, providerType));

      var genericMethod = WellKnownMembers.Queryable.ExtensionLock.MakeGenericMethod(new[] {typeof (TSource)});
      var expression = Expression.Call(null, genericMethod, new[] {source.Expression, Expression.Constant(lockMode), Expression.Constant(lockBehavior)});
      return source.Provider.CreateQuery<TSource>(expression);
    }

    /// <summary>
    /// Checks if <paramref name="source"/> value is contained in the specified list of values.
    /// </summary>
    /// <typeparam name="T">Type of value to check.</typeparam>
    /// <param name="source">Source value.</param>
    /// <param name="values">List of values to check.</param>
    /// <returns><see langword="True"/> if <paramref name="source"/> contains in the list of values, otherwise returns <see langword="false"/>.</returns>
    /// <remarks>LINQ translator detects this method and converts it to appropriate <see langword="Contains"/> method.</remarks>
    public static bool In<T>(this T source, params T[] values)
    {
      return In(source, (IEnumerable<T>)values);
    }

    /// <summary>
    /// Checks if <paramref name="source"/> value is contained in the specified list of values.
    /// </summary>
    /// <typeparam name="T">Type of value to check.</typeparam>
    /// <param name="source">Source value.</param>
    /// <param name="values">List of values to check.</param>
    /// <returns><see langword="True"/> if <paramref name="source"/> contains in the list of values, otherwise returns <see langword="false"/>.</returns>
    /// <remarks>LINQ translator detects this method and converts it to appropriate <see langword="Contains"/> method.</remarks>
    public static bool In<T>(this T source, IEnumerable<T> values)
    {
      if (values==null)
        return false;
      return values.Contains(source);
    }

    /// <summary>
    /// Checks if <paramref name="source"/> value is contained in the specified list of values.
    /// </summary>
    /// <typeparam name="T">Type of value to check.</typeparam>
    /// <param name="source">Source value.</param>
    /// <param name="algorithm">Translation algorithm.</param>
    /// <param name="values">List of values to check.</param>
    /// <returns><see langword="True"/> if <paramref name="source"/> contains in the list of values, otherwise returns <see langword="false"/>.</returns>
    /// <remarks>LINQ translator detects this method and converts it to appropriate <see langword="Contains"/> method.</remarks>
    public static bool In<T>(this T source, IncludeAlgorithm algorithm, params T[] values)
    {
      return In(source, algorithm, (IEnumerable<T>)values);
    }

    /// <summary>
    /// Checks if <paramref name="source"/> value is contained in the specified list of values.
    /// </summary>
    /// <typeparam name="T">Type of value to check.</typeparam>
    /// <param name="source">Source value.</param>
    /// <param name="algorithm">Translation algorithm.</param>
    /// <param name="values">List of values to check.</param>
    /// <returns><see langword="True"/> if <paramref name="source"/> contains in the list of values, otherwise returns <see langword="false"/>.</returns>
    /// <remarks>LINQ translator detects this method and converts it to appropriate <see langword="Contains"/> method.</remarks>
    public static bool In<T>(this T source, IncludeAlgorithm algorithm, IEnumerable<T> values)
    {
      if (values==null)
        return false;
      return values.Contains(source);
    }

    /// <summary>
    /// Correlates the elements of two sequences based on matching keys. 
    /// </summary>
    /// <typeparam name="TOuter">The type of the elements of the first sequence.</typeparam>
    /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
    /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
    /// <typeparam name="TResult">The type of the result elements.</typeparam>
    /// <param name="outer">The first sequence to join.</param>
    /// <param name="inner">The sequence to join to the first sequence.</param>
    /// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
    /// <param name="innerKeySelector">A function to extract the join key from each element of the second sequence.</param>
    /// <param name="resultSelector">A function to create a result element from two matching elements.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">One of provided arguments is <see langword="null" />.</exception>
    /// <exception cref="NotSupportedException">Queryable is not a <see cref="Xtensive.Orm.Linq"/> query.</exception>
    public static IQueryable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(this IQueryable<TOuter> outer, IEnumerable<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, TInner, TResult>> resultSelector)
    {
      ArgumentValidator.EnsureArgumentNotNull(outer, "outer");
      ArgumentValidator.EnsureArgumentNotNull(inner, "inner");
      ArgumentValidator.EnsureArgumentNotNull(outerKeySelector, "outerKeySelector");
      ArgumentValidator.EnsureArgumentNotNull(innerKeySelector, "innerKeySelector");
      ArgumentValidator.EnsureArgumentNotNull(resultSelector, "resultSelector");
      var errorMessage = Strings.ExLeftJoinDoesNotSupportQueryProviderOfTypeX;

      var outerProviderType = outer.Provider.GetType();
      if (outerProviderType!=typeof (QueryProvider))
        throw new NotSupportedException(String.Format(errorMessage, outerProviderType));

      var genericMethod = WellKnownMembers.Queryable.ExtensionLeftJoin.MakeGenericMethod(new[] {typeof (TOuter), typeof(TInner), typeof(TKey), typeof(TResult)});
      var expression = Expression.Call(null, genericMethod, new[] {outer.Expression, GetSourceExpression(inner), outerKeySelector, innerKeySelector, resultSelector});
      return outer.Provider.CreateQuery<TResult>(expression);
    }

    /// <summary>
    /// Removes the specified entities using <see cref="Session.Remove{T}"/> method of <see cref="Session"/>. 
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="entities">The entities.</param>
    ///  <exception cref="ReferentialIntegrityException">
    /// Entity is associated with another entity with <see cref="OnRemoveAction.Deny"/> on-remove action.
    /// </exception>
    [Obsolete("Use Session.Remove() instead.")]
    public static void Remove<T>([InstantHandle] this IEnumerable<T> entities)
      where T : IEntity
    {
      var session = Session.Current;
      if (session != null)
        session.Remove(entities);
      else {
        var items = entities.Where(e => e != null).ToList();
        if (items.Count == 0)
          return;
        items[0].Session.Remove(items);
      }
    }

    /// <summary>
    /// Runs query to database asynchronously  and returns completed task for other <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of elements in sequence.</typeparam>
    /// <param name="source">Query to run asynchronous.</param>
    /// <returns>A task which runs query.</returns>
    [Obsolete("Use AsAsync(IQueryable<t>) method instead.")]
    public static Task<IEnumerable<T>> AsAsyncTask<T>(this IQueryable<T> source)
    {
      return AsAsync(source, CancellationToken.None);
    }

    /// <summary>
    /// Runs query to database asynchronously  and returns completed task for other <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of elements in sequence.</typeparam>
    /// <param name="source">Query to run asynchronous.</param>
    /// <param name="cancellationToken">Token to cancel operation.</param>
    /// <returns>A task which runs query.</returns>
    [Obsolete("Use AsAsync(IQueryable<t>, CancellationToken) method instead.")]
    public static Task<IEnumerable<T>> AsAsyncTask<T>(this IQueryable<T> source, CancellationToken cancellationToken)
    {
      return AsAsync(source, cancellationToken);
    }

    /// <summary>
    /// Runs query to database asynchronously  and returns completed task for other <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of elements in sequence.</typeparam>
    /// <param name="source">Query to run asynchronous.</param>
    /// <returns>A task which runs query.</returns>
    public static Task<IEnumerable<T>> AsAsync<T>(this IQueryable<T> source)
    {
      return AsAsync(source, CancellationToken.None);
    }

    /// <summary>
    /// Runs query to database asynchronously  and returns completed task for other <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of elements in sequence.</typeparam>
    /// <param name="source">Query to run asynchronous.</param>
    /// <param name="cancellationToken">Token to cancel operation.</param>
    /// <returns>A task which runs query.</returns>
    public static Task<IEnumerable<T>> AsAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken)
    {
      var doProvider = source.Provider as QueryProvider;
      if (doProvider!=null)
        return doProvider.ExecuteAsync<IEnumerable<T>>(source.Expression, cancellationToken);
      return Task<IEnumerable<T>>.FromResult(source.AsEnumerable());
    }

    #region Private / internal members

    // ReSharper disable UnusedMember.Local

    private static IQueryable<TSource> CallTranslator<TSource>(MethodInfo methodInfo, IQueryable source, Expression fieldSelector, string errorMessage)
    {
      
      var providerType = source.Provider.GetType();
      if (providerType!=typeof (QueryProvider))
        throw new NotSupportedException(String.Format(errorMessage, providerType));

      var genericMethod = methodInfo.MakeGenericMethod(new[] {typeof (TSource)});
      var expression = Expression.Call(null, genericMethod, new[] {source.Expression, fieldSelector});
      return source.Provider.CreateQuery<TSource>(expression);
    }

    // ReSharper restore UnusedMember.Local

    private static Expression GetSourceExpression<TSource>(IEnumerable<TSource> source)
    {
      var queryable = source as IQueryable<TSource>;
      if (queryable!=null)
        return queryable.Expression;
      return Expression.Constant(source, typeof (IEnumerable<TSource>));
    }

    #endregion
  }
}
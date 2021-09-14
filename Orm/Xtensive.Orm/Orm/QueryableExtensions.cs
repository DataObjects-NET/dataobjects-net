// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2009.05.06

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Xtensive.Core;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Linq;
using Xtensive.Reflection;

namespace Xtensive.Orm
{
  /// <summary>
  /// Extends LINQ methods for <see cref="Xtensive.Orm.Linq"/> queries. 
  /// </summary>
  public static partial class QueryableExtensions
  {
    /// <summary>
    /// Tags query with given <paramref name="tag"/> string 
    /// (inserts string as comment in SQL statement) for 
    /// further query identification.
    /// </summary>
    /// <typeparam name="TSource">The type of the source element.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="tag">The unique tag to insert.</param>
    /// <returns>The same sequence, but with "comment" applied to query.</returns>
    public static IQueryable<TSource> Tag<TSource>(this IQueryable<TSource> source, string tag)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(tag, "tag");

      var errorMessage = Strings.ExTakeDoesNotSupportQueryProviderOfTypeX;
      var providerType = source.Provider.GetType();
      if (providerType != WellKnownOrmTypes.QueryProvider)
        throw new NotSupportedException(String.Format(errorMessage, providerType));

      var genericMethod = WellKnownMembers.Queryable.ExtensionTag.MakeGenericMethod(new[] { typeof(TSource) });
      var expression = Expression.Call(null, genericMethod, new[] { source.Expression, Expression.Constant(tag) });
      return source.Provider.CreateQuery<TSource>(expression);
    }
    
    /// <summary>
    /// Tags query with given <paramref name="tag"/> string 
    /// (inserts string as comment in SQL statement) for 
    /// further query identification.
    /// </summary>
    /// <param name="source">The source sequence.</param>
    /// <param name="tag">The unique tag to insert.</param>
    /// <returns>The same sequence, but with "comment" applied to query.</returns>
    public static IQueryable Tag(this IQueryable source, string tag)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(tag, "tag");

      var errorMessage = Strings.ExTakeDoesNotSupportQueryProviderOfTypeX;
      var providerType = source.Provider.GetType();
      if (providerType != WellKnownOrmTypes.QueryProvider)
        throw new NotSupportedException(String.Format(errorMessage, providerType));

      var genericMethod = WellKnownMembers.Queryable.ExtensionTag.MakeGenericMethod(new[] { source.ElementType });
      var expression = Expression.Call(null, genericMethod, new[] { source.Expression, Expression.Constant(tag) });
      return source.Provider.CreateQuery(expression);
    }

    /// <summary>
    /// Returns the number of elements in <paramref name="source"/> sequence.
    /// </summary>
    /// <param name="source">The source sequence.</param>
    public static int Count([InstantHandle] this IQueryable source)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      return (int) source.Provider.Execute(
        Expression.Call(
          WellKnownTypes.Queryable, nameof(Queryable.Count),
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
      if (providerType!=WellKnownOrmTypes.QueryProvider)
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
      if (providerType!=WellKnownOrmTypes.QueryProvider)
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
      if (providerType!=WellKnownOrmTypes.QueryProvider)
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
      if (providerType!=WellKnownOrmTypes.QueryProvider)
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
      if (providerType!=WellKnownOrmTypes.QueryProvider)
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
      if (outerProviderType!=WellKnownOrmTypes.QueryProvider)
        throw new NotSupportedException(String.Format(errorMessage, outerProviderType));

      var genericMethod = WellKnownMembers.Queryable.ExtensionLeftJoin.MakeGenericMethod(new[] {typeof (TOuter), typeof(TInner), typeof(TKey), typeof(TResult)});
      var expression = Expression.Call(null, genericMethod, new[] {outer.Expression, GetSourceExpression(inner), outerKeySelector, innerKeySelector, resultSelector});
      return outer.Provider.CreateQuery<TResult>(expression);
    }

    /// <summary>
    /// Runs query to database asynchronously  and returns completed task for other <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="T">Type of elements in sequence.</typeparam>
    /// <param name="source">Query to run asynchronous.</param>
    /// <returns>A task which runs query.</returns>
    [Obsolete("Use ExecuteAsync(IQueryable<T>) method instead.")]
    public static async Task<IEnumerable<T>> AsAsync<T>(this IQueryable<T> source) =>
      await ExecuteAsync(source, CancellationToken.None).ConfigureAwait(false);

    /// <summary>
    /// Runs query to database asynchronously  and returns completed task for other <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="T">Type of elements in sequence.</typeparam>
    /// <param name="source">Query to run asynchronous.</param>
    /// <param name="token">Token to cancel operation.</param>
    /// <returns>A task which runs query.</returns>
    [Obsolete("Use ExecuteAsync(IQueryable<T>, CancellationToken) method instead.")]
    public static async Task<IEnumerable<T>> AsAsync<T>(this IQueryable<T> source, CancellationToken token) =>
      await ExecuteAsync(source, token).ConfigureAwait(false);

    /// <summary>
    /// Runs query to database asynchronously and returns completed task for other <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="T">Type of elements in sequence.</typeparam>
    /// <param name="source">Query to run asynchronous.</param>
    /// <returns>A task which runs query.</returns>
    public static Task<QueryResult<T>> ExecuteAsync<T>(this IQueryable<T> source) =>
      ExecuteAsync(source, CancellationToken.None);

    /// <summary>
    /// Runs query to database asynchronously  and returns completed task for other <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <remarks>Multiple active operations in the same session instance are not supported. Use
    /// <see langword="await"/> to ensure that all asynchronous operations have completed before calling
    /// another method in this session.</remarks>
    /// <typeparam name="T">Type of elements in sequence.</typeparam>
    /// <param name="source">Query to run asynchronous.</param>
    /// <param name="cancellationToken">Token to cancel operation.</param>
    /// <returns>A task which runs query.</returns>
    public static async Task<QueryResult<T>> ExecuteAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken)
    {
      if (source.Provider is QueryProvider queryProvider) {
        return await queryProvider.ExecuteSequenceAsync<T>(source.Expression, cancellationToken).ConfigureAwait(false);
      }

      return new QueryResult<T>(source.AsEnumerable());
    }

    #region Private / internal members

    // ReSharper disable UnusedMember.Local

    private static IQueryable<TSource> CallTranslator<TSource>(MethodInfo methodInfo, IQueryable source, Expression fieldSelector, string errorMessage)
    {
      
      var providerType = source.Provider.GetType();
      if (providerType!=WellKnownOrmTypes.QueryProvider)
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

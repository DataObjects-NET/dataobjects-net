// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.05.06

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Linq
{
  /// <summary>
  /// Extends Linq methods for <see cref="Xtensive.Storage.Linq"/> query. 
  /// </summary>
  public static class QueryableExtensions
  {
    public static IQueryable<TSource> Take<TSource>(this IQueryable<TSource> source, Func<int> count)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(count, "count");

      var errorMessage = Resources.Strings.ExTakeDoesNotSupportQueryProviderOfTypeX;
      var providerType = source.Provider.GetType();
      if (providerType!=typeof (QueryProvider))
        throw new NotSupportedException(String.Format(errorMessage, providerType));

      var genericMethod = WellKnownMembers.QueryableExtensionTake.MakeGenericMethod(new[] {typeof (TSource)});
      var expression = Expression.Call(null, genericMethod, new[] {source.Expression, Expression.Constant(count)});
      return source.Provider.CreateQuery<TSource>(expression);
    }

    public static IQueryable<TSource> Skip<TSource>(this IQueryable<TSource> source, Func<int> count)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(count, "count");

      var errorMessage = Resources.Strings.ExSkipDoesNotSupportQueryProviderOfTypeX;
      var providerType = source.Provider.GetType();
      if (providerType!=typeof (QueryProvider))
        throw new NotSupportedException(String.Format(errorMessage, providerType));

      var genericMethod = WellKnownMembers.QueryableExtensionSkip.MakeGenericMethod(new[] {typeof (TSource)});
      var expression = Expression.Call(null, genericMethod, new[] {source.Expression, Expression.Constant(count)});
      return source.Provider.CreateQuery<TSource>(expression);
    }

    public static IQueryable<TSource> Lock<TSource>(this IQueryable<TSource> source, LockMode lockMode, LockBehavior lockBehavior)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(lockMode, "lockMode");
      ArgumentValidator.EnsureArgumentNotNull(lockBehavior, "lockBehavior");
      var errorMessage = Resources.Strings.ExLockDoesNotSupportQueryProviderOfTypeX;
      var providerType = source.Provider.GetType();
      if (providerType!=typeof (QueryProvider))
        throw new NotSupportedException(String.Format(errorMessage, providerType));

      var genericMethod = WellKnownMembers.QueryableExtensionLock.MakeGenericMethod(new[] {typeof (TSource)});
      var expression = Expression.Call(null, genericMethod, new[] {source.Expression, Expression.Constant(lockMode), Expression.Constant(lockBehavior)});
      return source.Provider.CreateQuery<TSource>(expression);
    }

    /// <summary>
    /// Checks if <paramref name="source"/> value contains in specified list of values.
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
    /// Checks if <paramref name="source"/> value contains in specified list of values.
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
    /// <exception cref="ArgumentNullException"><paramref name="outer"/> argument is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="inner"/> argument is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="outerKeySelector"/> argument is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="innerKeySelector"/> argument is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="resultSelector"/> argument is null.</exception>
    /// <exception cref="NotSupportedException">Queryable is not <see cref="Xtensive.Storage.Linq"/> query.</exception>
    public static IQueryable<TResult> JoinLeft<TOuter, TInner, TKey, TResult>(this IQueryable<TOuter> outer, IEnumerable<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector, Expression<Func<TOuter, TInner, TResult>> resultSelector)
    {
      ArgumentValidator.EnsureArgumentNotNull(outer, "outer");
      ArgumentValidator.EnsureArgumentNotNull(inner, "inner");
      ArgumentValidator.EnsureArgumentNotNull(outerKeySelector, "outerKeySelector");
      ArgumentValidator.EnsureArgumentNotNull(innerKeySelector, "innerKeySelector");
      ArgumentValidator.EnsureArgumentNotNull(resultSelector, "resultSelector");
      var errorMessage = Resources.Strings.ExJoinLeftDoesNotSupportQueryProviderOfTypeX;

      var outerProviderType = outer.Provider.GetType();
      if (outerProviderType!=typeof (QueryProvider))
        throw new NotSupportedException(String.Format(errorMessage, outerProviderType));

      var genericMethod = WellKnownMembers.QueryableExtensionJoinLeft.MakeGenericMethod(new[] {typeof (TOuter), typeof(TInner), typeof(TKey), typeof(TResult)});
      var expression = Expression.Call(null, genericMethod, new[] {outer.Expression, GetSourceExpression(inner), outerKeySelector, innerKeySelector, resultSelector});
      return outer.Provider.CreateQuery<TResult>(expression);
    }

    /// <exception cref="NotSupportedException">Queryable is not <see cref="Xtensive.Storage.Linq"/> query.</exception>
    private static IQueryable<TSource> CallTranslator<TSource>(MethodInfo methodInfo, IQueryable source, Expression fieldSelector, string errorMessage)
    {
      var providerType = source.Provider.GetType();
      if (providerType!=typeof (QueryProvider))
        throw new NotSupportedException(String.Format(errorMessage, providerType));

      var genericMethod = methodInfo.MakeGenericMethod(new[] {typeof (TSource)});
      var expression = Expression.Call(null, genericMethod, new[] {source.Expression, fieldSelector});
      return source.Provider.CreateQuery<TSource>(expression);
    }

    private static Expression GetSourceExpression<TSource>(IEnumerable<TSource> source)
    {
      var queryable = source as IQueryable<TSource>;
      if (queryable!=null)
        return queryable.Expression;
      return Expression.Constant(source, typeof (IEnumerable<TSource>));
    }
  }
}
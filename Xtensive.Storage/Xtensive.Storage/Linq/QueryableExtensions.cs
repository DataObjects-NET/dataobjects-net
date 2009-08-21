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

namespace Xtensive.Storage.Linq
{
  /// <summary>
  /// Extends Linq methods for <see cref="Xtensive.Storage.Linq"/> query.
  /// </summary>
  public static class QueryableExtensions
  {
    /// <summary>
    /// Fetches <see cref="Entity"/>, specified in <paramref name="selector"/>.
    /// This <see cref="Entity"/> will be queried along with base query.
    /// </summary>
    /// <typeparam name="TSource">Type of source.</typeparam>
    /// <param name="source">Source query.</param>
    /// <param name="selector"><see cref="Entity"/> selector.</param>
    /// <returns>The similar query. Only difference is request to storage. It will contains expanded <see cref="Entity"/> too.</returns>
    /// <exception cref="NotSupportedException">Queryable is not <see cref="Xtensive.Storage.Linq"/> query.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> argument is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="selector"/> argument is null.</exception>
    /// <remarks>Overrides <see cref="FieldAttribute.LazyLoad"/> setting for specified fields.</remarks>
    public static IQueryable<TSource> Prefetch<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, Entity>> selector)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(selector, "selector");
      var errorMessage = Resources.Strings.ExExpandDoesNotSupportQueryProviderOfTypeX;
      return CallTranslator<TSource>(WellKnownMembers.QueryableExpandEntity, source, selector, errorMessage);
    }


    /// <summary>
    /// Prefetches subquery, specified in <paramref name="selector"/>.
    /// This subquery will be queried along with base query.
    /// </summary>
    /// <typeparam name="TSource">Type of source.</typeparam>
    /// <param name="source">Source query.</param>
    /// <param name="selector">Subquery selector.</param>
    /// <returns>The similar query. Only difference is request to storage. It will contains expanded subquery too.</returns>
    /// <exception cref="NotSupportedException">Queryable is not <see cref="Xtensive.Storage.Linq"/> query.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> argument is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="selector"/> argument is null.</exception>
    public static IQueryable<TSource> Prefetch<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, IQueryable>> selector)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(selector, "selector");
      var errorMessage = Resources.Strings.ExExpandDoesNotSupportQueryProviderOfTypeX;
      return CallTranslator<TSource>(WellKnownMembers.QueryableExpandSubquery, source, selector, errorMessage);
    }

    /// <summary>
    /// Excludes field, specified in <paramref name="selector"/> in query. 
    /// </summary>
    /// <typeparam name="TSource">Type of source.</typeparam>
    /// <typeparam name="TKey">Type of field to exclude from query.</typeparam>
    /// <param name="source">Source query.</param>
    /// <param name="selector">Field selector.</param>
    /// <returns>The similar query. Only difference is request to storage. 
    /// It will not contains excluded field. This field will become <see cref="FieldAttribute.LazyLoad"/> field.</returns>
    /// <exception cref="NotSupportedException">Queryable is not <see cref="Xtensive.Storage.Linq"/> query.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> argument is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="selector"/> argument is null.</exception>
    /// <remarks>Overrides <see cref="FieldAttribute.LazyLoad"/> setting for specified field.</remarks>
    public static IQueryable<TSource> ExcludeFields<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> selector)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(selector, "selector");
      var errorMessage = Resources.Strings.ExExcludeFieldsDoesNotSupportQueryProviderOfTypeX;
      return CallTranslator<TSource>(WellKnownMembers.QueryableExcludeFields, source, selector, errorMessage);
    }

    /// <summary>
    /// Includes field, specified in <paramref name="selector"/> in query. 
    /// </summary>
    /// <typeparam name="TSource">Type of source.</typeparam>
    /// <typeparam name="TKey">Type of field to include into query.</typeparam>
    /// <param name="source">Source query.</param>
    /// <param name="selector">Field selector.</param>
    /// <returns>The similar query. Only difference is request to storage. 
    /// It will contains included field. This field will become non-<see cref="FieldAttribute.LazyLoad"/> field.</returns>
    /// <exception cref="NotSupportedException">Queryable is not <see cref="Xtensive.Storage.Linq"/> query.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> argument is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="selector"/> argument is null.</exception>
    /// <remarks>Overrides <see cref="FieldAttribute.LazyLoad"/> setting for specified field.</remarks>
    public static IQueryable<TSource> IncludeFields<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> selector)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(selector, "selector");
      var errorMessage = Resources.Strings.ExExcludeFieldsDoesNotSupportQueryProviderOfTypeX;
      return CallTranslator<TSource>(WellKnownMembers.QueryableIncludeFields, source, selector, errorMessage);
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

      var genericMethod = WellKnownMembers.QueryableJoinLeft.MakeGenericMethod(new[] {typeof (TOuter), typeof(TInner), typeof(TKey), typeof(TResult)});
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
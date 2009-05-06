// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.05.06

using System;
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
    /// Expands <see cref="EntitySet{TItem}"/>, specified in <paramref name="selector"/>.
    /// This <see cref="EntitySet{TItem}"/> will be queried along with source query.
    /// </summary>
    /// <typeparam name="TSource">Type of source.</typeparam>
    /// <param name="source">Source query.</param>
    /// <param name="selector"><see cref="EntitySet{TItem}"/> selector.</param>
    /// <returns>The similar query. Only difference is request to storage. It will contain expanded <see cref="EntitySet{TItem}"/> too.</returns>
    /// <exception cref="NotSupportedException">Queryable is not <see cref="Xtensive.Storage.Linq"/> query.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> argument is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="selector"/> argument is null.</exception>
    /// <remarks>Overrides <see cref="FieldAttribute.LazyLoad"/> setting for specified fields.</remarks>
    public static IQueryable<TSource> Expand<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, EntitySetBase>> selector)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(selector, "selector");
      var errorMessage = Resources.Strings.ExExpandDoesNotSupportQueryProviderOfTypeX;
      return CallTranslator<TSource>(source, selector, errorMessage);
    }

    /// <summary>
    /// Expands <see cref="Entity"/>, specified in <paramref name="selector"/>.
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
    public static IQueryable<TSource> Expand<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, Entity>> selector)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(selector, "selector");
      var errorMessage = Resources.Strings.ExExpandDoesNotSupportQueryProviderOfTypeX;
      return CallTranslator<TSource>(source, selector, errorMessage);
    }


    /// <summary>
    /// Expands subquery, specified in <paramref name="selector"/>.
    /// This subquery will be queried along with base query.
    /// </summary>
    /// <typeparam name="TSource">Type of source.</typeparam>
    /// <param name="source">Source query.</param>
    /// <param name="selector">Subquery selector.</param>
    /// <returns>The similar query. Only difference is request to storage. It will contains expanded subquery too.</returns>
    /// <exception cref="NotSupportedException">Queryable is not <see cref="Xtensive.Storage.Linq"/> query.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> argument is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="selector"/> argument is null.</exception>
    public static IQueryable<TSource> Expand<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, IQueryable>> selector)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(selector, "selector");
      var errorMessage = Resources.Strings.ExExpandDoesNotSupportQueryProviderOfTypeX;
      return CallTranslator<TSource>(source, selector, errorMessage);
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
      return CallTranslator<TSource>(source, selector, errorMessage);
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
      return CallTranslator<TSource>(source, selector, errorMessage);
    }

    /// <exception cref="NotSupportedException">Queryable is not <see cref="Xtensive.Storage.Linq"/> query.</exception>
    private static IQueryable<TSource> CallTranslator<TSource>(IQueryable source, Expression fieldSelector, string errorMessage)
    {
      var providerType = source.Provider.GetType();
      if (providerType!=typeof (QueryProvider))
        throw new NotSupportedException(String.Format(errorMessage, providerType));

      var methodInfo = ((MethodInfo) MethodBase.GetCurrentMethod());
      var genericMethod = methodInfo.MakeGenericMethod(new[] {typeof (TSource)});
      var expression = Expression.Call(null, genericMethod, new[] {source.Expression, fieldSelector});
      return source.Provider.CreateQuery<TSource>(expression);
    }
  }
}
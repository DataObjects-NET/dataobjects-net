// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.05.06

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core.Reflection;
using Xtensive.Core;
using System.Collections.Generic;

namespace Xtensive.Storage.Linq
{
  /// <summary>
  /// Extends Linq methods for <see cref="Xtensive.Storage.Linq"/> query.
  /// </summary>
  public static class QueryableExtensions
  {
    /// <summary>
    /// Expands <see cref="EntitySet{TItem}"/>  fields, specified in <paramref name="fieldSelectors"/>.
    /// Can be used  
    /// These fields will be queried along with base query.
    /// </summary>
    /// <typeparam name="TSource">Type of source.</typeparam>
    /// <param name="source">Source queryable to expand fields for.</param>
    /// <param name="fieldSelectors">List of field selectors.</param>
    /// <returns>The similar query. Only difference is request to storage. It will contains expanded <see cref="EntitySet{TItem}"/>s too.</returns>
    /// <exception cref="NotSupportedException">Queryable is not <see cref="Xtensive.Storage.Linq"/> query.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> argument is null.</exception>
    /// <remarks>Overrides <see cref="FieldAttribute.LazyLoad"/> setting for specified fields.</remarks>
    public static IQueryable<TSource> ExpandFields<TSource>(this IQueryable<TSource> source, params Expression<Func<TSource, EntitySetBase>>[] fieldSelectors)
    {
      var errorMessage = Resources.Strings.ExExpandFieldsDoesNotSupportQueryProviderOfTypeX;
      return CallTranslator(source, fieldSelectors, errorMessage);
    }

    /// <summary>
    /// Expands fields, specified in <paramref name="fieldSelectors"/>.
    /// Can be used  
    /// These fields will be queried along with base query.
    /// </summary>
    /// <typeparam name="TSource">Type of source.</typeparam>
    /// <param name="source">Source queryable to expand fields for.</param>
    /// <param name="fieldSelectors">List of field selectors.</param>
    /// <returns>The similar query. Only difference is request to storage. It will contains expanded <see cref="EntitySet{TItem}"/>s too.</returns>
    /// <exception cref="NotSupportedException">Queryable is not <see cref="Xtensive.Storage.Linq"/> query.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> argument is null.</exception>
    /// <remarks>Overrides <see cref="FieldAttribute.LazyLoad"/> setting for specified fields.</remarks>
    public static IQueryable<TSource> ExpandFields<TSource>(this IQueryable<TSource> source, params Expression<Func<TSource, Entity>>[] fieldSelectors)
    {
      var errorMessage = Resources.Strings.ExExpandFieldsDoesNotSupportQueryProviderOfTypeX;
      return CallTranslator(source, fieldSelectors, errorMessage);
    }

    /// <summary>
    /// Includes fields, specified in <paramref name="fieldSelectors"/> in query. 
    /// </summary>
    /// <typeparam name="TSource">Type of source.</typeparam>
    /// <param name="source">Source queryable to exclude fields for.</param>
    /// <param name="fieldSelectors">List of field selectors.</param>
    /// <returns>The similar query. Only difference is request to storage. It will not contains excluded fields.</returns>
    /// <exception cref="NotSupportedException">Queryable is not <see cref="Xtensive.Storage.Linq"/> query.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> argument is null.</exception>
    /// <remarks>Overrides <see cref="FieldAttribute.LazyLoad"/> setting for specified fields.</remarks>
    public static IQueryable<TSource> ExcludeFields<TSource>(this IQueryable<TSource> source, params Expression<Func<TSource, EntitySetBase>>[] fieldSelectors)
    {
      var errorMessage = Resources.Strings.ExExcludeFieldsDoesNotSupportQueryProviderOfTypeX;
      return CallTranslator(source, fieldSelectors, errorMessage);
    }

    /// <summary>
    /// Includes fields, specified in <paramref name="fieldSelectors"/> from query.
    /// These fields will become <see cref="FieldAttribute.LazyLoad"/> fields. 
    /// </summary>
    /// <typeparam name="TSource">Type of source.</typeparam>
    /// <param name="source">Source queryable to include fields for.</param>
    /// <param name="fieldSelectors">List of field selectors.</param>
    /// <returns>The similar query. Only difference is request to storage. It will contains included fields too.</returns>
    /// <exception cref="NotSupportedException">Queryable is not <see cref="Xtensive.Storage.Linq"/> query.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> argument is null.</exception>
    /// <remarks>Overrides <see cref="FieldAttribute.LazyLoad"/> setting for specified fields.</remarks>
    public static IQueryable<TSource> IncludeFields<TSource>(this IQueryable<TSource> source, params Expression<Func<TSource, EntitySetBase>>[] fieldSelectors)
    {
      var errorMessage = Resources.Strings.ExExcludeFieldsDoesNotSupportQueryProviderOfTypeX;
      return CallTranslator(source, fieldSelectors, errorMessage);
    }

    /// <exception cref="NotSupportedException">Queryable is not <see cref="Xtensive.Storage.Linq"/> query.</exception>
    private static IQueryable<TSource> CallTranslator<TSource>(IQueryable<TSource> source, Expression[] fieldSelectors, string errorMessage)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      var providerType = source.Provider.GetType();
      if (providerType!=typeof (QueryProvider))
        throw new NotSupportedException(String.Format(errorMessage, providerType));

      if (fieldSelectors==null || fieldSelectors.Length==0)
        return source;

      var parameters = new List<Expression>(fieldSelectors.Length+1) {
        source.Expression
      };
      parameters.AddRange(fieldSelectors);

      return source.Provider.CreateQuery<TSource>(Expression.Call(
        null
        , ((MethodInfo) MethodBase.GetCurrentMethod()).MakeGenericMethod(new[] {typeof (TSource)})
        , parameters.ToArray()));
    }
  }
}
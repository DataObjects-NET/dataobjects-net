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
    /// Expands fields, specified in <paramref name="fieldSelectors"/>. 
    /// These fields will be queried and cached along with base query.
    /// Overrides <see cref="FieldAttribute.LazyLoad"/> setting for specified fields.
    /// </summary>
    /// <typeparam name="TSource">Type of source.</typeparam>
    /// <param name="source">Source queryable to expand fields for.</param>
    /// <param name="fieldSelectors">List of field selectors.</param>
    /// <returns>The similar query. Only difference is request to storage. It will contains expanded fields too.</returns>
    /// <exception cref="NotSupportedException">Queryable is not <see cref="Xtensive.Storage.Linq"/> query</exception>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> argument is null.</exception>
    public static IQueryable<TSource> ExpandFields<TSource>(this IQueryable<TSource> source, params Expression<Func<TSource, EntitySetBase>>[] fieldSelectors)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      var queryType = source.GetType();
      if (!queryType.IsOfGenericType(typeof (Query<>)))
        throw new NotSupportedException(String.Format(Resources.Strings.ExExpandFieldsDoesNotSupportQueryableOfTypeX, queryType));

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

    /// <summary>
    /// Includes fields, specified in <paramref name="fieldSelectors"/> in query. 
    /// Overrides <see cref="FieldAttribute.LazyLoad"/> setting for specified fields.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="queryable"></param>
    /// <returns></returns>
    public static IQueryable<TSource> ExcludeFields<TSource>(this IQueryable<TSource> queryable, params Expression<Func<TSource, EntitySetBase>>[] fieldSelectors)
    {
      var queryType = queryable.GetType();
      if (!queryType.IsOfGenericType(typeof (Query<>)))
        throw new NotSupportedException(String.Format(Resources.Strings.ExExcludeFieldsDoesNotSupportQueryableOfTypeX, queryType));

      throw new NotImplementedException();
    }

    /// <summary>
    /// Includes fields, specified in <paramref name="fieldSelectors"/> in query. 
    /// Overrides <see cref="FieldAttribute.LazyLoad"/> setting.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="queryable"></param>
    /// <returns></returns>
    public static IQueryable<TSource> IncludeFields<TSource>(this IQueryable<TSource> queryable, params Expression<Func<TSource, EntitySetBase>>[] fieldSelectors)
    {
      var queryType = queryable.GetType();
      if (!queryType.IsOfGenericType(typeof (Query<>)))
        throw new NotSupportedException(String.Format(Resources.Strings.ExIncludeFieldsDoesNotSupportQueryableOfTypeX, queryType));

      throw new NotImplementedException();
    }
  }
}
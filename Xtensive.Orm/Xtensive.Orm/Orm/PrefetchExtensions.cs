// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.09.30

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Collections;
using Xtensive.Orm.Internals.Prefetch;
using Xtensive.Orm.Linq;

namespace Xtensive.Orm
{
  /// <summary>
  /// Contains extension methods allowing prefetch fields of an <see cref="Entity"/>.
  /// </summary>
  public static class PrefetchExtensions
  {
    /// <summary>
    /// Creates <see cref="Prefetcher{T,TElement}"/> for the specified <paramref name="source"/> and 
    /// registers the prefetch of the field specified by <paramref name="expression"/>.
    /// </summary>
    /// <typeparam name="TElement">The type of the element of the source sequence.</typeparam>
    /// <typeparam name="TFieldValue">The type of the field's value to be prefetched.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="expression">The expression specifying a field to be prefetched.</param>
    /// <returns>A newly created <see cref="Prefetcher{T,TElement}"/>.</returns>
    public static IEnumerable<TElement> Prefetch<TElement, TFieldValue>(
      this IQueryable<TElement> source,
      Expression<Func<TElement, TFieldValue>> expression,
      params Expression<Func<TElement, TFieldValue>>[] expressions)
    {
      var queryProvider = source.Provider as QueryProvider;
      if (queryProvider == null)
        return Prefetch((IEnumerable<TElement>) source, expression, expressions);
      var facade = new PrefetchFacade<TElement>(queryProvider.Session, source);
      return expressions.Aggregate(facade.RegisterPath(expression), (current, e) => current.RegisterPath(e));
    }

    public static IEnumerable<TElement> Prefetch<TElement, TFieldValue>(
      this IEnumerable<TElement> source,
      Expression<Func<TElement, TFieldValue>> expression, 
      params Expression<Func<TElement, TFieldValue>>[] expressions)
    {
      return Prefetch(source, Session.Demand(), expression, expressions);
    }

    public static IEnumerable<TElement> Prefetch<TElement, TFieldValue>(
      this IEnumerable<TElement> source,
      Session session,
      Expression<Func<TElement, TFieldValue>> expression,
      params Expression<Func<TElement, TFieldValue>>[] expressions)
    {
      var facade = source as PrefetchFacade<TElement>;
      if (facade != null)
        return expressions.Aggregate(facade.RegisterPath(expression), (current, e) => current.RegisterPath(e));

      facade = new PrefetchFacade<TElement>(session, source);
      return expressions.Aggregate(facade.RegisterPath(expression), (current, e) => current.RegisterPath(e));
    }
  }
}
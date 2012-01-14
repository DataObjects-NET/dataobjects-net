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
    /// Registers fields specified by <paramref name="expression"/> for prefetch.
    /// </summary>
    /// <typeparam name="TElement">The type of the element of the source sequence.</typeparam>
    /// <typeparam name="TFieldValue">The type of the field's value to be prefetched.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="expression">The expression specifying a field to be prefetched.</param>
    /// <returns>An <see cref="IEnumerable{TElement}"/> of source items.</returns>
    public static IEnumerable<TElement> Prefetch<TElement, TFieldValue>(
      this IQueryable<TElement> source,
      Expression<Func<TElement, TFieldValue>> expression)
    {
      var queryProvider = source.Provider as QueryProvider;
      if (queryProvider == null)
        return Prefetch(source, Session.Demand(), expression);
      return new PrefetchFacade<TElement>(queryProvider.Session, source).RegisterPath(expression);
    }

    /// <summary>
    /// Registers fields specified by <paramref name="expression"/> for prefetch.
    /// </summary>
    /// <typeparam name="TElement">The type of the element of the source sequence.</typeparam>
    /// <typeparam name="TFieldValue">The type of the field's value to be prefetched.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="expression">The expression specifying a field to be prefetched.</param>
    /// <returns>An <see cref="IEnumerable{TElement}"/> of source items.</returns>
    public static IEnumerable<TElement> Prefetch<TElement, TFieldValue>(
      this IEnumerable<TElement> source,
      Expression<Func<TElement, TFieldValue>> expression)
    {
      var facade = source as PrefetchFacade<TElement>;
      if (facade != null)
        return facade.RegisterPath(expression);
      return Prefetch(source, Session.Demand(), expression);
    }

    /// <summary>
    /// Creates <see cref="PrefetchFacade{T}"/> for the specified <paramref name="source"/> and
    /// registers the prefetch of the field specified by <paramref name="expression"/>.
    /// </summary>
    /// <typeparam name="TElement">The type of the element of the source sequence.</typeparam>
    /// <typeparam name="TFieldValue">The type of the field's value to be prefetched.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="session">The session.</param>
    /// <param name="expression">The expression specifying a field to be prefetched.</param>
    /// <returns>A newly created <see cref="PrefetchFacade{T}"/>.</returns>
    public static IEnumerable<TElement> Prefetch<TElement, TFieldValue>(
      this IEnumerable<TElement> source,
      Session session,
      Expression<Func<TElement, TFieldValue>> expression)
    {
      var facade = source as PrefetchFacade<TElement>;
      if (facade != null)
        return facade.RegisterPath(expression);
      return new PrefetchFacade<TElement>(session, source).RegisterPath(expression);
    }
  }
}
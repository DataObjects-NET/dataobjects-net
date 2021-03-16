// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexander Nikolaev
// Created:    2009.09.30

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
    public static PrefetchQuery<TElement> Prefetch<TElement, TFieldValue>(
      this IQueryable<TElement> source,
      Expression<Func<TElement, TFieldValue>> expression)
    {
      if (source.Provider is QueryProvider queryProvider) {
        return new PrefetchQuery<TElement>(queryProvider.Session, source).RegisterPath(expression);
      }

      return Prefetch(source, Session.Demand(), expression);
    }

    /// <summary>
    /// Registers fields specified by <paramref name="expression"/> for prefetch.
    /// </summary>
    /// <typeparam name="TElement">The type of the element of the source sequence.</typeparam>
    /// <typeparam name="TFieldValue">The type of the field's value to be prefetched.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="expression">The expression specifying a field to be prefetched.</param>
    /// <returns>An <see cref="IEnumerable{TElement}"/> of source items.</returns>
    public static PrefetchQuery<TElement> Prefetch<TElement, TFieldValue>(
      this DelayedQuery<TElement> source,
      Expression<Func<TElement, TFieldValue>> expression)
    {
      return new PrefetchQuery<TElement>(source.Session, source).RegisterPath(expression);
    }

    /// <summary>
    /// Registers fields specified by <paramref name="expression"/> for prefetch.
    /// </summary>
    /// <typeparam name="TElement">The type of the element of the source sequence.</typeparam>
    /// <typeparam name="TFieldValue">The type of the field's value to be prefetched.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="expression">The expression specifying a field to be prefetched.</param>
    /// <returns>An <see cref="IEnumerable{TElement}"/> of source items.</returns>
    public static PrefetchQuery<TElement> Prefetch<TElement, TFieldValue>(
      this QueryResult<TElement> source,
      Expression<Func<TElement, TFieldValue>> expression)
    {
      var session = source.Session;
      if (session != null) {
        return new PrefetchQuery<TElement>(session, source).RegisterPath(expression);
      }
      return new PrefetchQuery<TElement>(Session.Demand(), source).RegisterPath(expression);
    }



    /// <summary>
    /// Registers fields specified by <paramref name="expression"/> for prefetch.
    /// </summary>
    /// <typeparam name="TElement">The type of the element of the source sequence.</typeparam>
    /// <typeparam name="TFieldValue">The type of the field's value to be prefetched.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="expression">The expression specifying a field to be prefetched.</param>
    /// <returns>An <see cref="IEnumerable{TElement}"/> of source items.</returns>
    public static PrefetchQuery<TElement> Prefetch<TElement, TFieldValue>(
      this IEnumerable<TElement> source,
      Expression<Func<TElement, TFieldValue>> expression)
    {
      if (source is PrefetchQuery<TElement> prefetchQuery) {
        return prefetchQuery.RegisterPath(expression);
      }

      return Prefetch(source, Session.Demand(), expression);
    }

    /// <summary>
    /// Creates <see cref="PrefetchQuery{T}"/> for the specified <paramref name="source"/> and
    /// registers the prefetch of the field specified by <paramref name="expression"/>.
    /// </summary>
    /// <typeparam name="TElement">The type of the element of the source sequence.</typeparam>
    /// <typeparam name="TFieldValue">The type of the field's value to be prefetched.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="session">The session.</param>
    /// <param name="expression">The expression specifying a field to be prefetched.</param>
    /// <returns>A newly created <see cref="PrefetchQuery{T}"/>.</returns>
    public static PrefetchQuery<TElement> Prefetch<TElement, TFieldValue>(
      this IEnumerable<TElement> source,
      Session session,
      Expression<Func<TElement, TFieldValue>> expression)
    {
      if (source is PrefetchQuery<TElement> prefetchQuery) {
        return prefetchQuery.RegisterPath(expression);
      }
      return new PrefetchQuery<TElement>(session, source).RegisterPath(expression);
    }
  }
}
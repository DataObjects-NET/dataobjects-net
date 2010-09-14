// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.09.30

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core.Collections;
using Xtensive.Storage.Internals.Prefetch;

namespace Xtensive.Storage
{
  /// <summary>
  /// Contains extension methods allowing prefetch fields of an <see cref="Entity"/>.
  /// </summary>
  public static class PrefetchEnumerableExtensions
  {
    /// <summary>
    /// Prefetches specified <paramref name="source"/> sequence.
    /// </summary>
    /// <param name="source">The source sequence.</param>
    /// <returns>Prefetched sequence.</returns>
    public static IEnumerable<Entity> Prefetch(this IEnumerable<Key> source)
    {
      return source.Prefetch(Session.Demand());
    }

    /// <summary>
    /// Prefetches specified <paramref name="source"/> sequence.
    /// </summary>
    /// <param name="source">The source sequence.</param>
    /// <param name="session">The session.</param>
    /// <returns>Prefetched sequence.</returns>
    public static IEnumerable<Entity> Prefetch(this IEnumerable<Key> source, Session session)
    {
      return source
        .Prefetch<Key>(key => key)
        .Select(key => session.Query.SingleOrDefault(key))
        .ToTransactional(); // Necessary, because there is Query.SingleOrDefault(key).
    }

    /// <summary>
    /// Creates <see cref="Prefetcher{T,TElement}"/> for the specified <paramref name="source"/>.
    /// </summary>
    /// <typeparam name="TElement">The type of the element of the source sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="keyExtractor">The <see cref="Key"/> extractor.</param>
    /// <returns>A newly created <see cref="Prefetcher{T,TElement}"/>.</returns>
    public static Prefetcher<Entity, TElement> Prefetch<TElement>(this IEnumerable<TElement> source,
      Func<TElement, Key> keyExtractor)
    {
      return new Prefetcher<Entity, TElement>(source, keyExtractor);
    }

    /// <summary>
    /// Creates <see cref="Prefetcher{T,TElement}"/> for the specified <paramref name="source"/>.
    /// </summary>
    /// <typeparam name="T">The type containing fields which can be registered for prefetch.</typeparam>
    /// <typeparam name="TElement">The type of the element of the source sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="keyExtractor">The <see cref="Key"/> extractor.</param>
    /// <returns>A newly created <see cref="Prefetcher{T,TElement}"/>.</returns>
    public static Prefetcher<T, TElement> Prefetch<T, TElement>(this IEnumerable<TElement> source,
      Func<TElement, Key> keyExtractor)
      where T : IEntity
    {
      return new Prefetcher<T, TElement>(source, keyExtractor);
    }

    /// <summary>
    /// Creates <see cref="Prefetcher{T,TElement}"/> for the specified <paramref name="source"/> and 
    /// registers the prefetch of the field specified by <paramref name="expression"/>.
    /// </summary>
    /// <typeparam name="TElement">The type of the element of the source sequence.</typeparam>
    /// <typeparam name="TFieldValue">The type of the field's value to be prefetched.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="expression">The expression specifying a field to be prefetched.</param>
    /// <returns>A newly created <see cref="Prefetcher{T,TElement}"/>.</returns>
    public static Prefetcher<TElement, TElement> Prefetch<TElement, TFieldValue>(
      this IEnumerable<TElement> source, Expression<Func<TElement, TFieldValue>> expression)
      where TElement : IEntity
    {
      return new Prefetcher<TElement, TElement>(source, element => element.Key)
        .Prefetch(expression);
    }

    /// <summary>
    /// Creates <see cref="Prefetcher{T,TElement}"/> for the specified <paramref name="source"/>,
    /// registers the prefetch of the field specified by <paramref name="expression"/> and
    /// registers the delegate prefetching fields of an object referenced by element of
    /// the sequence.
    /// </summary>
    /// <typeparam name="TElement">The type of the element of the source sequence.</typeparam>
    /// <typeparam name="TItem">The type of a sequence item.</typeparam>
    /// <param name="expression">The expression specifying a field to be prefetched.</param>
    /// <param name="nestedPrefetcher">The delegate prefetching fields of a referenced object.</param>
    /// <returns><see langword="this"/></returns>
    public static Prefetcher<TElement, TElement> Prefetch<TElement, TItem>(
      this IEnumerable<TElement> source, Expression<Func<TElement, IEnumerable<TItem>>> expression,
      Func<IEnumerable<TItem>, IEnumerable<TItem>> nestedPrefetcher)
      where TElement : IEntity
    {
      return new Prefetcher<TElement, TElement>(source, element => element.Key)
        .Prefetch(expression, nestedPrefetcher);
    }

    /// <summary>
    /// Creates <see cref="Prefetcher{T,TElement}"/> for the specified <paramref name="source"/> and 
    /// registers the prefetch of the field specified by <paramref name="expression"/>.
    /// </summary>
    /// <typeparam name="TElement">The type of the element of the source sequence.</typeparam>
    /// <typeparam name="TItem">The type of a sequence item.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="expression">The expression specifying a field to be prefetched.</param>
    /// <param name="entitySetItemCountLimit">The limit of count of items to be loaded during the 
    /// prefetch of <see cref="EntitySet{T}"/>.</param>
    /// <returns>A newly created <see cref="Prefetcher{T,TElement}"/>.</returns>
    public static Prefetcher<TElement, TElement> Prefetch<TElement, TItem>(
      this IEnumerable<TElement> source, Expression<Func<TElement, IEnumerable<TItem>>> expression,
      int entitySetItemCountLimit)
      where TElement : IEntity
    {
      return new Prefetcher<TElement, TElement>(source, element => element.Key)
        .Prefetch(expression, entitySetItemCountLimit);
    }

    /// <summary>
    /// Creates <see cref="Prefetcher{T,TElement}"/> for the specified <paramref name="source"/>, 
    /// registers the delegate prefetching fields of an object referenced by element of
    /// the sequence specified by <paramref name="selector"/>.
    /// </summary>
    /// <typeparam name="TElement">The type of the element of the source sequence.</typeparam>
    /// <typeparam name="TSelectorResult">The result's type of a referenced object's selector.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="selector">The selector of a sequence.</param>
    /// <param name="nestedPrefetcher">The delegate prefetching fields of a referenced object.</param>
    /// <returns>A newly created <see cref="Prefetcher{T,TElement}"/>.</returns>
    public static Prefetcher<TElement, TElement> PrefetchMany<TElement, TSelectorResult>(
      this IEnumerable<TElement> source, Func<TElement,IEnumerable<TSelectorResult>> selector,
      Func<IEnumerable<TSelectorResult>, IEnumerable<TSelectorResult>> nestedPrefetcher)
      where TElement : IEntity
    {
      return new Prefetcher<TElement, TElement>(source, element => element.Key)
        .PrefetchMany(selector, nestedPrefetcher);
    }

    /// <summary>
    /// Creates <see cref="Prefetcher{T,TElement}"/> for the specified <paramref name="source"/>, 
    /// registers the delegate prefetching fields of an object referenced by the value
    /// specified by <paramref name="selector"/>.
    /// </summary>
    /// <typeparam name="TElement">The type of the element of the source sequence.</typeparam>
    /// <typeparam name="TSelectorResult">The result's type of a referenced object's selector.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="selector">The selector of a field value.</param>
    /// <param name="nestedPrefetcher">The delegate prefetching fields of a referenced object.</param>
    /// <returns>A newly created <see cref="Prefetcher{T,TElement}"/>.</returns>
    /// <remarks>This methods allows to select value of type which does not implement 
    /// <see cref="IEnumerable{T}"/>. The result of the <paramref name="selector"/> is transformed 
    /// to <see cref="IEnumerable{T}"/> by <see cref="EnumerableUtils.One{TItem}"/>.</remarks>
    public static Prefetcher<TElement, TElement> PrefetchSingle<TElement, TSelectorResult>(
      this IEnumerable<TElement> source, Func<TElement,TSelectorResult> selector,
      Func<IEnumerable<TSelectorResult>,IEnumerable<TSelectorResult>> nestedPrefetcher)
      where TElement : IEntity
      where TSelectorResult : IEntity
    {
      return new Prefetcher<TElement, TElement>(source, element => element.Key)
        .PrefetchSingle(selector, nestedPrefetcher);
    }
  }
}
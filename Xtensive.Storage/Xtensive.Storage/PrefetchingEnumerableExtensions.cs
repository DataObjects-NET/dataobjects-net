// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.09.30

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core.Collections;
using Xtensive.Storage.Internals;

namespace Xtensive.Storage
{
  /// <summary>
  /// Contains extension methods allowing prefetch fields of an <see cref="Entity"/>.
  /// </summary>
  public static class PrefetchingEnumerableExtensions
  {
    /// <summary>
    /// Creates <see cref="Prefetcher{T,TElement}"/> for the specified source.
    /// </summary>
    /// <typeparam name="T">The type containing fields which can be registered for prefetch.</typeparam>
    /// <typeparam name="TElement">The type of the element of the source sequence.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="keyExtractor">The <see cref="Key"/> extractor.</param>
    /// <returns>A newly created <see cref="Prefetcher{T,TElement}"/>.</returns>
    public static Prefetcher<T, TElement> Prefetch<T, TElement>(this IEnumerable<TElement> source,
      Func<TElement, Key> keyExtractor)
      where T : Entity
    {
      return new Prefetcher<T, TElement>(source, keyExtractor);
    }

    /// <summary>
    /// Creates <see cref="Prefetcher{T,TElement}"/> for the specified source and 
    /// registers the prefetch of the field specified by <paramref name="expression"/>.
    /// </summary>
    /// <typeparam name="TElement">The type of the element of the source sequence.</typeparam>
    /// <typeparam name="TFieldValue">The type of the field's value to be prefetched.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="expression">The expression specifying a field to be prefetched.</param>
    /// <returns>A newly created <see cref="Prefetcher{T,TElement}"/>.</returns>
    public static Prefetcher<TElement, TElement> Prefetch<TElement, TFieldValue>(
      this IEnumerable<TElement> source, Expression<Func<TElement, TFieldValue>> expression)
      where TElement : Entity
    {
      return new Prefetcher<TElement, TElement>(source, element => element.Key).Prefetch(expression);
    }

    /// <summary>
    /// Creates <see cref="Prefetcher{T,TElement}"/> for the specified source and 
    /// registers the prefetch of the field specified by <paramref name="expression"/>.
    /// </summary>
    /// <typeparam name="TElement">The type of the element of the source sequence.</typeparam>
    /// <typeparam name="TFieldValue">The type of the field's value to be prefetched.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="expression">The expression specifying a field to be prefetched.</param>
    /// <param name="entitySetItemCountLimit">The limit of count of items to be loaded during the 
    /// prefetch of <see cref="EntitySet{T}"/>.</param>
    /// <returns>A newly created <see cref="Prefetcher{T,TElement}"/>.</returns>
    public static Prefetcher<TElement, TElement> Prefetch<TElement, TFieldValue>(
      this IEnumerable<TElement> source, Expression<Func<TElement, TFieldValue>> expression,
      int entitySetItemCountLimit)
      where TElement : Entity
    {
      return new Prefetcher<TElement, TElement>(source, element => element.Key)
        .Prefetch(expression, entitySetItemCountLimit);
    }

    /// <summary>
    /// Creates <see cref="Prefetcher{T,TElement}"/> for the specified source, 
    /// registers the prefetch of the field specified by <paramref name="expression"/> and 
    /// registers the delegate prefetching fields of an object referenced by element of the source sequence.
    /// </summary>
    /// <typeparam name="TElement">The type of the element of the source sequence.</typeparam>
    /// <typeparam name="TFieldValue">The type of the field's value to be prefetched.</typeparam>
    /// <typeparam name="TSelectorResult">The result's type of a referenced object's selector.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="expression">The expression specifying a field to be prefetched.</param>
    /// <param name="selector">The selector of a referenced object.</param>
    /// <param name="prefetchManyFunc">The delegate prefetching fields of a referenced object.</param>
    /// <returns>A newly created <see cref="Prefetcher{T,TElement}"/>.</returns>
    public static Prefetcher<TElement, TElement> PrefetchMany<TElement, TFieldValue, TSelectorResult>(
      this IEnumerable<TElement> source, Expression<Func<TElement, TFieldValue>> expression,
      Func<TFieldValue,IEnumerable<TSelectorResult>> selector,
      Func<IEnumerable<TSelectorResult>, IEnumerable<TSelectorResult>> prefetchManyFunc)
      where TElement : Entity
    {
      return new Prefetcher<TElement, TElement>(source, element => element.Key)
        .PrefetchMany(expression, selector, prefetchManyFunc);
    }

    /// <summary>
    /// Creates <see cref="Prefetcher{T,TElement}"/> for the specified source, 
    /// registers the prefetch of the field specified by <paramref name="expression"/> and 
    /// registers the delegate prefetching fields of an object referenced by element of the source sequence.
    /// </summary>
    /// <typeparam name="TElement">The type of the element of the source sequence.</typeparam>
    /// <typeparam name="TFieldValue">The type of the field's value to be prefetched.</typeparam>
    /// <typeparam name="TSelectorResult">The result's type of a referenced object's selector.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="expression">The expression specifying a field to be prefetched.</param>
    /// <param name="entitySetItemCountLimit">The limit of count of items to be loaded during the 
    /// prefetch of <see cref="EntitySet{TItem}"/>.</param>
    /// <param name="selector">The selector of a referenced object.</param>
    /// <param name="prefetchManyFunc">The delegate prefetching fields of a referenced object.</param>
    /// <returns>A newly created <see cref="Prefetcher{T,TElement}"/>.</returns>
    public static Prefetcher<TElement, TElement> PrefetchMany<TElement, TFieldValue, TSelectorResult>(
      this IEnumerable<TElement> source, Expression<Func<TElement, TFieldValue>> expression,
      int entitySetItemCountLimit, Func<TFieldValue, IEnumerable<TSelectorResult>> selector,
      Func<IEnumerable<TSelectorResult>, IEnumerable<TSelectorResult>> prefetchManyFunc)
      where TElement : Entity
    {
      return new Prefetcher<TElement, TElement>(source, element => element.Key)
        .PrefetchMany(expression, entitySetItemCountLimit, selector, prefetchManyFunc);
    }

    /// <summary>
    /// Creates <see cref="Prefetcher{T,TElement}"/> for the specified source, 
    /// registers the prefetch of the field specified by <paramref name="expression"/> and 
    /// registers the delegate prefetching fields of an object referenced by element of the source sequence.
    /// </summary>
    /// <typeparam name="TElement">The type of the element of the source sequence.</typeparam>
    /// <typeparam name="TFieldValue">The type of the field's value to be prefetched.</typeparam>
    /// <typeparam name="TSelectorResult">The result's type of a referenced object's selector.</typeparam>
    /// <param name="source">The source.</param>
    /// <param name="expression">The expression specifying a field to be prefetched.</param>
    /// <param name="selector">The selector of a referenced object.</param>
    /// <param name="prefetchManyFunc">The delegate prefetching fields of a referenced object.</param>
    /// <returns>A newly created <see cref="Prefetcher{T,TElement}"/>.</returns>
    /// <remarks>This methods allows to select value of type which does not implement 
    /// <see cref="IEnumerable{T}"/>. The result of the <paramref name="selector"/> is transformed 
    /// to <see cref="IEnumerable{T}"/> by <see cref="EnumerableUtils.One{TItem}"/>.</remarks>
    public static Prefetcher<TElement, TElement> PrefetchSingle<TElement, TFieldValue, TSelectorResult>(
      this IEnumerable<TElement> source, Expression<Func<TElement, TFieldValue>> expression,
      Func<TFieldValue,TSelectorResult> selector,
      Func<IEnumerable<TSelectorResult>,IEnumerable<TSelectorResult>> prefetchManyFunc)
      where TElement : Entity
      where TSelectorResult : Entity
    {
      return new Prefetcher<TElement, TElement>(source, element => element.Key)
        .PrefetchSingle(expression, selector, prefetchManyFunc);
    }
  }
}
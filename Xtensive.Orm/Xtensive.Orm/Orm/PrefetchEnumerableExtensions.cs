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

namespace Xtensive.Orm
{
  /// <summary>
  /// Contains extension methods allowing prefetch fields of an <see cref="Entity"/>.
  /// </summary>
  public static class PrefetchEnumerableExtensions
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
      this IEnumerable<TElement> source, 
      Expression<Func<TElement, TFieldValue>> expression)
      where TElement : IEntity
    {
      throw new NotImplementedException();
//      return new Prefetcher<TElement, TElement>(source, element => element.Key)
//        .Prefetch(expression);
    }
  }
}
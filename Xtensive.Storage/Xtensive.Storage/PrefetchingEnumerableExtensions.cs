// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.09.30

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Storage.Internals;

namespace Xtensive.Storage
{
  public static class PrefetchingEnumerableExtensions
  {
    public static Prefetcher<TItem, TElement> Prefetch<TItem, TElement>(this IEnumerable<TElement> source,
      Func<TElement, Key> keyExtractor)
    {
      return new Prefetcher<TItem, TElement>(source, keyExtractor);
    }

    public static void Prefetch<TItem, TElement, TResult>(this Prefetcher<TItem, TElement> prefetcher,
      Expression<Func<TItem, TResult>> expression)
    {
      prefetcher.Prefetch(expression);
    }

    public static Prefetcher<TElement, TElement> Prefetch<TElement, TResult>(
      this IEnumerable<TElement> source, Expression<Func<TElement, TResult>> expression)
      where TElement : Entity
    {
      return new Prefetcher<TElement, TElement>(source, element => element.Key);
    }
  }
}
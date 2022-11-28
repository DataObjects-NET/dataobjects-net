// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.11.13

using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Collections
{
  /// <summary>
  /// Defines contract for a filterable collection of <typeparamref name="TItem"/>s.
  /// </summary>
  /// <typeparam name="TFilter">The type of the filter.</typeparam>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  public interface IFilterable<TFilter, TItem>
  {
    /// <summary>
    /// Finds the items from initial collection according to specified filter <paramref name="criteria"/>.
    /// </summary>
    /// <param name="criteria">The object to filter initial collection with.</param>
    /// <returns><see cref="IEnumerable{TItem}"/> object.</returns>
    IEnumerable<TItem> Find(TFilter criteria);

    /// <summary>
    /// Finds the items from initial collection according to specified filter <paramref name="criteria"/>.
    /// </summary>
    /// <param name="criteria">The object to filter initial collection with.</param>
    /// <param name="matchType">Type of the match.</param>
    /// <returns><see cref="IEnumerable{TItem}"/> object.</returns>
    IEnumerable<TItem> Find(TFilter criteria, MatchType matchType);
  }
}
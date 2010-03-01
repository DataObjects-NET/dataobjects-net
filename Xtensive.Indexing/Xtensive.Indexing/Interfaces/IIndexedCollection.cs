// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.11.08

using System.Collections.Generic;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Collection having a set of automatically maintained indexes contract.
  /// </summary>
  /// <typeparam name="TItem">The type of collection items.</typeparam>
  public interface IIndexedCollection<TItem>: ICollection<TItem>
  {
    /// <summary>
    /// Gets the set of collection indexes.
    /// </summary>
    /// <value>The set of indexes.</value>
    CollectionIndexSet<TItem> Indexes { get; }
  }
}
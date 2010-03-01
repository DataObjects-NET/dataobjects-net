// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.09.10

using System.Collections.Generic;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Non-unique index contract.
  /// </summary>
  /// <typeparam name="TKey">The type of the index key.</typeparam>
  /// <typeparam name="TItem">The type of the item (should include both key and value).</typeparam>
  public interface INonUniqueIndex<TKey, TItem>: IOrderedIndex<TKey, TItem>
  {
    /// <summary>
    /// Gets the items with the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns><see cref="IEnumerable{T}"/> of <typeparamref name="TItem"/> instances.</returns>
    IEnumerable<TItem> GetItems(TKey key);
  }
}
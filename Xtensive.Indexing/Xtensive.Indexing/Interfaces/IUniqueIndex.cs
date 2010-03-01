// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.12

using System;
using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Unique index contract.
  /// </summary>
  /// <typeparam name="TKey">The type of the index key.</typeparam>
  /// <typeparam name="TItem">The type of the item (should include both key and value).</typeparam>
  public interface IUniqueIndex<TKey, TItem> : 
    IIndex<TKey, TItem>
  {
    /// <summary>
    /// Gets the item by its key.
    /// </summary>
    /// <value>The element with the specified key.</value>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null reference.</exception>
    /// <exception cref="KeyNotFoundException">The property is retrieved and <paramref name="key"/> is not found.</exception>
    TItem GetItem(TKey key);
  }
}
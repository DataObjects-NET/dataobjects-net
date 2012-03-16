// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.18

using System;
using Xtensive.Core;
using Xtensive.Indexing.Measures;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Base generic index interface.
  /// </summary>
  /// <typeparam name="TKey">The type of the index key.</typeparam>
  /// <typeparam name="TItem">The type of the item (should include both key and value).</typeparam>
  public interface IIndex<TKey, TItem> : IIndex,
    IHasKeyExtractor<TKey, TItem>,
    IMeasurable<TItem>,
    IHasSize
  {
    /// <summary>
    /// Adds the element to the index.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <exception cref="ArgumentNullException"><paramref name="item"/> is null reference.</exception>
    void Add(TItem item);

    /// <summary>
    /// Removes the <paramref name="item"/> from the index.
    /// </summary>
    /// <param name="item">Item to remove.</param>
    /// <returns><see langword="True"/> if the item is found and removed;
    /// otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="item"/> is null reference.</exception>
    bool Remove(TItem item);

    /// <summary>
    /// Replaces the <paramref name="item"/> using extracted key.
    /// </summary>
    /// <param name="item">Item to replace.</param>
    /// <exception cref="ArgumentNullException"><paramref name="item"/> is null reference.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Item with extracted key could not be found.</exception>
    void Replace(TItem item);

    /// <summary>
    /// Removes the item(s) from the index by its (their) <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Key of the item(s) to remove.</param>
    /// <returns><see langword="True"/> if the item(s) with specified key is found and removed;
    /// otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null reference.</exception>
    bool RemoveKey(TKey key);

    ///<summary>
    /// Determines whether the index contains specified item.
    ///</summary>
    ///<returns>
    /// <see langword="true"/> if this instance contains the item; otherwise, <see langword="false"/>.
    ///</returns>
    ///<param name="item">The item to check for the containment.</param>
    /// <exception cref="ArgumentNullException"><paramref name="item"/> is null reference.</exception>
    bool Contains(TItem item);

    ///<summary>
    /// Determines whether the index contains specified key.
    ///</summary>
    ///<returns>
    /// <see langword="true"/> if this instance contains the key; otherwise, <see langword="false"/>.
    ///</returns>
    ///<param name="key">The key to check for the containment.</param>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null reference.</exception>
    bool ContainsKey(TKey key);
  }
}
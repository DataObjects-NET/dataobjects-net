// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.22

using System.Collections.Generic;

namespace Xtensive.Collections
{
  /// <summary>
  /// Generic set of items.
  /// </summary>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  public interface ISet<TItem> : ICollection<TItem>
  {
    /// <summary>
    /// Gets the specified item.
    /// </summary>
    /// <value>The item.</value>
    TItem this[TItem item] { get; }

    /// <summary>
    /// Gets the <see cref="IEqualityComparer{T}"/> object 
    /// that is used to determine equality for the values in this instance.
    /// </summary>
    /// <value>The <see cref="IEqualityComparer{T}"/> object 
    /// that is used to determine equality for the values in this instance.</value>
    IEqualityComparer<TItem> Comparer { get; }

    /// <summary>
    /// Adds the specified element to the <see cref="ISet{TItem}"/>.
    /// </summary>
    /// <param name="item">Item to add to the set.</param>
    /// <returns><see langword="True"/> if the element is added to the <see cref="ISet{TItem}"/> object; otherwise, <see langword="false"/>.</returns>
    new bool Add(TItem item);
  }
}
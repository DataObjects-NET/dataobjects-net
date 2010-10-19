// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.17

using System.Collections.Specialized;
using Xtensive.Collections;

namespace Xtensive.Modelling
{
  /// <summary>
  /// <see cref="Node"/> collection.
  /// </summary>
  public interface INodeCollection : IPathNode, 
    ICountable, 
    INotifyCollectionChanged
  {
    /// <summary>
    /// Gets the count of items.
    /// </summary>
    new int Count { get; }

    /// <summary>
    /// An indexer that provides access to collection items by their index.
    /// </summary>
    Node this[int index] { get; }

    /// <summary>
    /// An indexer that provides access to collection items by their names.
    /// Returns <see langword="null"/> if there is no such item.
    /// </summary>
    Node this[string name] { get; }

    /// <summary>
    /// Gets the item with the specified <see cref="Node.Name"/>.
    /// </summary>
    /// <param name="name">The <see cref="Node.Name"/> of the item to get.</param>
    /// <param name="value">Item, if it is found; otherwise <see langword="null"/>.</param>
    /// <returns>
    /// <see langword="true"/> if item is found by specified <paramref name="name"/>;
    /// otherwise <see langword="false"/>.
    /// </returns>
    bool TryGetValue(string name, out Node value);

    /// <summary>
    /// Determines whether collection contains
    /// an item with the specified <see cref="Node.Name"/>.
    /// </summary>
    /// <param name="name">The <see cref="Node.Name"/> of the item to find.</param>
    /// <returns>
    /// <see langword="true"/> if this collection contains
    /// an item with the specified <see cref="Node.Name"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool Contains(string name);

    /// <summary>
    /// Gets the temporary name (it isn't used in this collection).
    /// </summary>
    /// <returns>Temporary name.</returns>
    string GetTemporaryName();

    /// <summary>
    /// Clears the collection.
    /// </summary>
    void Clear();
  }
}
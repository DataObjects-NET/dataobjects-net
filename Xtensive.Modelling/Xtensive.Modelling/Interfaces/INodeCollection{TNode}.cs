// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.17

using System.Collections.Generic;

namespace Xtensive.Modelling
{
  /// <summary>
  /// Typed <see cref="Node"/> collection.
  /// </summary>
  /// <typeparam name="TNode">The type of the node.</typeparam>
  public interface INodeCollection<TNode> : INodeCollection
    where TNode: Node
  {
    /// <summary>
    /// An indexer that provides access to collection items by their names.
    /// Returns <see langword="null"/> if there is no such item.
    /// </summary>
    new TNode this[string name] { get; }

    /// <summary>
    /// Gets the item with the specified <see cref="Node.Name"/>.
    /// </summary>
    /// <param name="name">The <see cref="Node.Name"/> of the item to get.</param>
    /// <param name="value">Item, if it is found; otherwise <see langword="null"/>.</param>
    /// <returns>
    /// <see langword="true"/> if item is found by specified <paramref name="name"/>;
    /// otherwise <see langword="false"/>.
    /// </returns>
    bool TryGetValue(string name, out TNode value);
  }
}
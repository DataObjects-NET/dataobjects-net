// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.18

using System.Collections.Generic;
using Xtensive.Collections;

namespace Xtensive.Modelling
{
  /// <summary>
  /// Typed <see cref="Node"/> collection.
  /// </summary>
  /// <typeparam name="TNode">The type of the collection item.</typeparam>
  public interface INodeCollection<TNode> : INodeCollection,
    ICountable<TNode>
    where TNode : Node
  {
    /// <summary>
    /// An indexer that provides access to collection items by their index.
    /// </summary>
    new TNode this[int index] { get; }

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
    new bool TryGetValue(string name, out Node value);
  }
}
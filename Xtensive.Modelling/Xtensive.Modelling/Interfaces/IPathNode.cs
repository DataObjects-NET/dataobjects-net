// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.17

using Xtensive.Core;

namespace Xtensive.Modelling
{
  /// <summary>
  /// Base class for any model node or node collection.
  /// </summary>
  public interface IPathNode : ILockable
  {
    /// <summary>
    /// Gets the name of this node.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the parent node.
    /// </summary>
    Node Parent { get; }

    /// <summary>
    /// Gets the model this node belongs to.
    /// </summary>
    Model Model { get; }

    /// <summary>
    /// Gets the path to this node.
    /// </summary>
    string Path { get; }

    /// <summary>
    /// Gets the child node by its path.
    /// </summary>
    /// <param name="path">Path to the node to get.</param>
    /// <returns>Path node, if found;
    /// otherwise, <see langword="null" />.</returns>
    IPathNode GetChild(string path);
  }
}
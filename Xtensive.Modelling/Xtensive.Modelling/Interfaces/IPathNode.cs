// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.17

using Xtensive.Core;

namespace Xtensive.Modelling
{
  /// <summary>
  /// Base class for any model node or node collection (path node).
  /// </summary>
  public interface IPathNode : ILockable
  {
    /// <summary>
    /// Gets or sets the name of this node.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Gets escaped <see cref="Name"/> of this node.
    /// </summary>
    string EscapedName { get; }

    /// <summary>
    /// Gets or sets the parent node.
    /// </summary>
    Node Parent { get; set; }

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
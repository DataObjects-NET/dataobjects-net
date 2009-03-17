// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.17

using Xtensive.Core.Notifications;

namespace Xtensive.Modelling
{
  /// <summary>
  /// Node interface.
  /// </summary>
  public interface INode : IPathNode
  {
    /// <summary>
    /// Gets the index of the node in the <see cref="NodeCollection"/>.
    /// </summary>
    int Index { get; }
    
    /// <summary>
    /// Gets the parent node collection this node belongs to.
    /// </summary>
    /// <returns>
    /// Parent node collection;
    /// <see langword="null"/>, if none.
    /// </returns>
    INodeCollection NodeCollection { get; }

    /// <summary>
    /// Gets the parent node collection this node should belong to.
    /// </summary>
    /// <param name="parent">Parent node to get the node collection of.</param>
    /// <returns>
    /// Parent node collection;
    /// <see langword="null"/>, if none.
    /// </returns>
    INodeCollection GetNodeCollection(INode parent);

    /// <summary>
    /// Renames the node.
    /// </summary>
    /// <param name="newName">The new name.</param>
    void Rename(string newName);

    /// <summary>
    /// Moves the node.
    /// </summary>
    /// <param name="newParent">The new parent.</param>
    /// <param name="newName">The new name.</param>
    /// <param name="newIndex">The new index.</param>
    void Move(INode newParent, string newName, int newIndex);

    /// <summary>
    /// Moves the node.
    /// </summary>
    /// <param name="newParent">The new parent.</param>
    void Move(INode newParent);

    /// <summary>
    /// Moves the node.
    /// </summary>
    /// <param name="newIndex">The new index.</param>
    void Move(int newIndex);

    /// <summary>
    /// Removes the node.
    /// </summary>
    void Remove();
  }
}
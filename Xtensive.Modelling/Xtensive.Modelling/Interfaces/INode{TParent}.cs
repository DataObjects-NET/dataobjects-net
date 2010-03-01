// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.19

namespace Xtensive.Modelling
{
  /// <summary>
  /// Node with specified <see cref="IPathNode.Parent"/> type.
  /// </summary>
  /// <typeparam name="TParent">The type of the parent.</typeparam>
  public interface INode<TParent> : INode,
    IPathNode<TParent>
    where TParent : Node
  {
    /// <summary>
    /// Gets or sets the parent node.
    /// </summary>
    new TParent Parent { get; set; }

    /// <summary>
    /// Moves the node.
    /// </summary>
    /// <param name="newParent">The new parent.</param>
    /// <param name="newName">The new name.</param>
    /// <param name="newIndex">The new index.</param>
    void Move(TParent newParent, string newName, int newIndex);
  }
}
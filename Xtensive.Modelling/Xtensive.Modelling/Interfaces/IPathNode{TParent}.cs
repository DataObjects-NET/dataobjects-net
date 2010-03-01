// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.18

namespace Xtensive.Modelling
{
  /// <summary>
  /// Path node with specified <see cref="IPathNode.Parent"/> type.
  /// </summary>
  /// <typeparam name="TParent">The type of the parent.</typeparam>
  public interface IPathNode<TParent> : IPathNode
    where TParent : Node
  {
    /// <summary>
    /// Gets the parent node.
    /// </summary>
    new TParent Parent { get; }
  }
}
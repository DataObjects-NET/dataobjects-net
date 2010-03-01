// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.18

namespace Xtensive.Modelling
{
  /// <summary>
  /// Path node with specified <see cref="IPathNode.Model"/> 
  /// and <see cref="IPathNode.Parent"/> types.
  /// </summary>
  /// <typeparam name="TParent">The type of the parent.</typeparam>
  /// <typeparam name="TModel">The type of the model.</typeparam>
  public interface IPathNode<TParent, TModel> : IPathNode<TParent>
    where TParent : Node
    where TModel : IModel
  {
    /// <summary>
    /// Gets the model this node belongs to.
    /// </summary>
    new TModel Model { get; }
  }
}
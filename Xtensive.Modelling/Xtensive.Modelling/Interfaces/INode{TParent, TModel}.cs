// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.18

namespace Xtensive.Modelling
{
  /// <summary>
  /// Node with specified <see cref="Node.Model"/> 
  /// and <see cref="Node.Parent"/> types.
  /// </summary>
  /// <typeparam name="TParent">The type of the parent.</typeparam>
  /// <typeparam name="TModel">The type of the model.</typeparam>
  public interface INode<TParent, TModel> : INode<TParent>
    where TParent : Node
    where TModel : Model
  {
    /// <summary>
    /// Gets the model this node belongs to.
    /// </summary>
    new TModel Model { get; }
  }
}
// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.16

using System;
using System.Diagnostics;
using Xtensive.Indexing.Storage.Resources;
using Xtensive.Storage.Model;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// Abstract base class for node with specified <see cref="Node.Parent"/> type.
  /// </summary>
  /// <typeparam name="TNode">The type of the node.</typeparam>
  /// <typeparam name="TParent">The type of the parent.</typeparam>
  [Serializable]
  public abstract class Node<TNode, TParent> : Node<TParent>
    where TNode : Node
    where TParent : Node
  {
    /// <summary>
    /// Gets the parent node collection this node can be stored in.
    /// </summary>
    /// <returns>Parent node collection;
    /// <see langword="null" />, if none.</returns>
    protected virtual NodeCollection<TNode, TParent> GetParentNodeCollection()
    {
      return null;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">Item with specified name already exists 
    /// in parent node collection.</exception>
    protected override void ValidateName(string newName)
    {
      base.ValidateName(newName);
      var c = GetParentNodeCollection();
      if (c==null)
        return;
      TNode node;
      if (!c.TryGetValue(newName, out node))
        return;
      if (node==this)
        return;
      throw new ArgumentException(String.Format(
        Strings.ExItemWithNameXAlreadyExists, newName), newName);
    }


    // Constructors

    /// <inheritdoc/>
    protected Node(TParent parent, string name)
      : base(parent, name)
    {
    }

    /// <inheritdoc/>
    protected Node(string name)
      : base(name)
    {
    }

    /// <inheritdoc/>
    protected Node()
    {
    }
  }
}
// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using Xtensive.Modelling;

namespace Xtensive.Orm.Upgrade.Model
{
  /// <summary>
  /// An abstract base class for any collection of storage model nodes.
  /// </summary>
  /// <typeparam name="TNode">The type of the node.</typeparam>
  /// <typeparam name="TParent">The type of the parent node.</typeparam>
  [Serializable]
  public abstract class NodeCollectionBase<TNode, TParent> : NodeCollection<TNode, TParent, StorageInfo>
    where TNode : Node
    where TParent : Node
  {
    // Constructors

    /// <inheritdoc/>
    protected NodeCollectionBase(Node parent, string name)
      : base(parent, name)
    {
    }
  }
}
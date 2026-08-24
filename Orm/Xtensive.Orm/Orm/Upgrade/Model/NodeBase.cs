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
  /// A base class for all nodes in storage model.
  /// </summary>
  /// <typeparam name="TParent">The type of the parent node.</typeparam>
  public abstract class NodeBase<TParent> : Node<TParent, StorageModel>
    where TParent : Node
  {

    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="parent">The parent.</param>
    /// <param name="name">The name.</param>
    protected NodeBase(TParent parent, string name)
      : base(parent, name)
    {
    }
  }
}
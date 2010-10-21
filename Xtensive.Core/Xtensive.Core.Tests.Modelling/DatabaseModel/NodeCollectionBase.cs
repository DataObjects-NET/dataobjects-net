// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.20

using System;
using Xtensive.Modelling;

namespace Xtensive.Core.Tests.Modelling.DatabaseModel
{
  [Serializable]
  public class NodeCollectionBase<TNode, TParent> : NodeCollection<TNode, TParent, Server>
    where TNode : Node
    where TParent : Node
  {
    public NodeCollectionBase(Node parent, string name)
      : base(parent, name)
    {
    }
  }
}
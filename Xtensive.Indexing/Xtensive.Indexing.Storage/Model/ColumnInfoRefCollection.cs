// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using Xtensive.Modelling;
using System.Diagnostics;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// A collection of <see cref="ColumnInfoRef{TParent}"/>.
  /// </summary>
  /// <typeparam name="TParent">The type of the parent node.</typeparam>
  [Serializable]
  public class ColumnInfoRefCollection<TParent> : NodeCollectionBase<ColumnInfoRef<TParent>, TParent>
    where TParent : Node
  {
    // Constructors

    public ColumnInfoRefCollection(Node parent, string name)
      : base(parent, name)
    {
    }
  }
}
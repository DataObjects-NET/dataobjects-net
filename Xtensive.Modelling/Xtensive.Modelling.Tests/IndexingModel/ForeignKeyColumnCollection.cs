// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.20

using System;
using System.Diagnostics;

namespace Xtensive.Modelling.Tests.IndexingModel
{
  /// <summary>
  /// A collection of <see cref="ForeignKeyColumnRef"/> instances.
  /// </summary>
  [Serializable]
  public class ForeignKeyColumnCollection : NodeCollectionBase<ForeignKeyColumnRef, ForeignKeyInfo>
  {

    /// <inheritdoc/>
    public ForeignKeyColumnCollection(Node parent, string name)
      : base(parent, name)
    {
    }
  }
}
// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;

namespace Xtensive.Orm.Tests.Core.Modelling.IndexingModel
{
  /// <summary>
  /// A collection of <see cref="PrimaryKeyColumnRef"/> instances.
  /// </summary>
  [Serializable]
  public sealed class PrimaryKeyColumnRefCollection : NodeCollectionBase<PrimaryKeyColumnRef, SecondaryIndexInfo>
  {
    // Constructors
    
    public PrimaryKeyColumnRefCollection(SecondaryIndexInfo parent)
      : base(parent, "PrimaryKeyColumns")
    {
    }
  }
}
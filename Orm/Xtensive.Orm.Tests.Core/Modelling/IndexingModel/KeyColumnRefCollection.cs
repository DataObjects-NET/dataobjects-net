// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;

namespace Xtensive.Orm.Tests.Core.Modelling.IndexingModel
{
  /// <summary>
  /// A collection of <see cref="KeyColumnRef"/> instances.
  /// </summary>
  [Serializable]
  public sealed class KeyColumnRefCollection : NodeCollectionBase<KeyColumnRef, IndexInfo>
  {
    // Constructors

    public KeyColumnRefCollection(IndexInfo parent)
      : base(parent, "KeyColumns")
    {
    }
  }
}
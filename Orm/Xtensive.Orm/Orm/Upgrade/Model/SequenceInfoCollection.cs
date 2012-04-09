// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.29

using System;

using Xtensive.Modelling;

namespace Xtensive.Orm.Upgrade.Model
{
  /// <summary>
  /// The collection of <see cref="StorageSequenceInfo"/> instances.
  /// </summary>
  [Serializable]
  public sealed class SequenceInfoCollection : NodeCollectionBase<StorageSequenceInfo, StorageModel>,
    IUnorderedNodeCollection
  {
    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="parent">The storage.</param>
    public SequenceInfoCollection(StorageModel parent)
      : base(parent, "Sequences")
    {
    }
  }
}
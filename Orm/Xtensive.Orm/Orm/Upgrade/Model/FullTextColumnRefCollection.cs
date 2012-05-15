// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.01.14

using System;
using System.Diagnostics;

using Xtensive.Modelling;

namespace Xtensive.Orm.Upgrade.Model
{
  /// <summary>
  /// A collection of <see cref="FullTextColumnRef"/> instances.
  /// </summary>
  [Serializable]
  public sealed class FullTextColumnRefCollection : NodeCollectionBase<FullTextColumnRef, StorageFullTextIndexInfo>
  {

    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public FullTextColumnRefCollection(StorageFullTextIndexInfo parent)
      : base(parent, "Columns")
    {
    }
  }
}
// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.24

using System;
using System.Diagnostics;
using Xtensive.Modelling;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// A collection of <see cref="TableInfo"/>.
  /// </summary>
  [Serializable]
  public class TableInfoCollection: NodeCollectionBase<TableInfo, StorageInfo>
  {

    // Constructors

    /// <inheritdoc/>
    public TableInfoCollection(StorageInfo parent)
      : base(parent, "Tables")
    {
    }
  }
}
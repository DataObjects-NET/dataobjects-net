// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2010.01.25

using System;

using Xtensive.Modelling;

namespace Xtensive.Orm.Upgrade.Model
{
  /// <summary>
  /// A collection of full-text indexes.
  /// </summary>
  [Serializable]
  public class FullTextIndexInfoCollection: NodeCollectionBase<StorageFullTextIndexInfo, TableInfo>,
    IUnorderedNodeCollection
  {
    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="table">The table.</param>
    public FullTextIndexInfoCollection(TableInfo table)
      : base(table, "FullTextIndexes")
    {
    }
  }
}
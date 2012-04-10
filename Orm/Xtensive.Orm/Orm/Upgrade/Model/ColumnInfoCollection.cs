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
  /// Column collection.
  /// </summary>
  [Serializable]
  public sealed class ColumnInfoCollection : NodeCollectionBase<StorageColumnInfo, TableInfo>,
    IUnorderedNodeCollection
  {
    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="parent">The parent.</param>
    public ColumnInfoCollection(Node parent)
      : base(parent, "Columns")
    {
    }
  }
}
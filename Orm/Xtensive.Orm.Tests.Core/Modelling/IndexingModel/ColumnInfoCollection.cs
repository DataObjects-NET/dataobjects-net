// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using Xtensive.Modelling;

namespace Xtensive.Orm.Tests.Core.Modelling.IndexingModel
{
  /// <summary>
  /// Column collection.
  /// </summary>
  [Serializable]
  public sealed class ColumnInfoCollection : NodeCollectionBase<ColumnInfo, TableInfo>
  {
    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="parent">The parent.</param>
    public ColumnInfoCollection(Node parent)
      : base(parent, "Columns")
    {
    }
  }
}
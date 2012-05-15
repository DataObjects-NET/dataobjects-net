// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.23

using System;

using Xtensive.Modelling;
using Xtensive.Orm.Upgrade.Model;

namespace Xtensive.Orm.Upgrade.Model
{
  /// <summary>
  /// Foreign key collection.
  /// </summary>
  [Serializable]
  public sealed class ForeignKeyCollection : NodeCollectionBase<ForeignKeyInfo, TableInfo>, 
    IUnorderedNodeCollection
  {
    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="table">The table.</param>
    public ForeignKeyCollection(TableInfo table)
      : base(table, "ForeignKeys")
    {
    }
  }
}
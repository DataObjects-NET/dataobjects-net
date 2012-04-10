// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.20

using System;

using Xtensive.Modelling;

namespace Xtensive.Orm.Upgrade.Model
{
  /// <summary>
  /// References to foreign key column.
  /// </summary>
  [Serializable]
  public sealed class ForeignKeyColumnRef : Ref<StorageColumnInfo, ForeignKeyInfo>
  {
    
    protected override Nesting CreateNesting()
    {
      return new Nesting<ForeignKeyColumnRef, ForeignKeyInfo, ForeignKeyColumnCollection>(
        this, "ForeignKeyColumns");
    }


    // Constructors

    
    public ForeignKeyColumnRef(ForeignKeyInfo parent)
      : base(parent)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="parent">The foreign key.</param>
    /// <param name="column">The column.</param>
    
    public ForeignKeyColumnRef(ForeignKeyInfo parent, StorageColumnInfo column)
      : base(parent)
    {
      Value = column;
    }
  }
}
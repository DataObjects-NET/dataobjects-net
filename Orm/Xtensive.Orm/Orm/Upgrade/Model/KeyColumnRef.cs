// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using Xtensive.Core;

using Xtensive.Modelling;

namespace Xtensive.Orm.Upgrade.Model
{
  /// <summary>
  /// References to key column.
  /// </summary>
  [Serializable]
  public sealed class KeyColumnRef : KeyColumnRef<StorageIndexInfo>
  {
    
    protected override Nesting CreateNesting()
    {
      return new Nesting<KeyColumnRef, StorageIndexInfo, KeyColumnRefCollection>(this, "KeyColumns");
    }


    // Constructors

    
    public KeyColumnRef(StorageIndexInfo parent)
      : base(parent)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="parent">The parent index.</param>
    /// <param name="column">The referenced column.</param>
    public KeyColumnRef(StorageIndexInfo parent, StorageColumnInfo column)
      : base(parent, column)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="parent">The parent index.</param>
    /// <param name="column">The referenced column.</param>
    /// <param name="direction">The direction.</param>
    public KeyColumnRef(StorageIndexInfo parent, StorageColumnInfo column, Direction direction)
      : base(parent, column, direction)
    {
    }
  }
}
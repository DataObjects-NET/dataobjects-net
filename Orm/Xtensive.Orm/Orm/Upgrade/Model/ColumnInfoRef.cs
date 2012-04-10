// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;


namespace Xtensive.Orm.Upgrade.Model
{
  /// <summary>
  /// An abstract base class for all columns refs.
  /// </summary>
  [Serializable]
  public abstract class ColumnInfoRef<TParent> : Ref<StorageColumnInfo, TParent>
    where TParent: NodeBase<TableInfo>
  {
    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="parent">The parent index.</param>
    /// <inheritdoc/>
    protected ColumnInfoRef(TParent parent)
      : base(parent)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="parent">The parent index.</param>
    /// <param name="column">The column.</param>
    protected ColumnInfoRef(TParent parent, StorageColumnInfo column)
      : base(parent)
    {
      Value = column;
    }
  }
}
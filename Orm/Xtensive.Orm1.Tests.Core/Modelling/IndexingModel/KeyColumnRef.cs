// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using Xtensive.Core;
using Xtensive.Modelling;

namespace Xtensive.Orm.Tests.Core.Modelling.IndexingModel
{
  /// <summary>
  /// References to key column.
  /// </summary>
  [Serializable]
  public sealed class KeyColumnRef: KeyColumnRef<IndexInfo>
  {
    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<KeyColumnRef, IndexInfo, KeyColumnRefCollection>(this, "KeyColumns");
    }


    // Constructors

    public KeyColumnRef(IndexInfo parent)
      : base(parent)
    {
    }

    public KeyColumnRef(IndexInfo parent, ColumnInfo column)
      : base(parent, column)
    {
    }

    public KeyColumnRef(IndexInfo parent, ColumnInfo column, Direction direction)
      : base(parent, column, direction)
    {
    }
  }
}
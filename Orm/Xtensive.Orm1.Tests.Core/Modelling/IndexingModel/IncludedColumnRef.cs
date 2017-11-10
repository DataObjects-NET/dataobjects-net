// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.24

using System;
using Xtensive.Modelling;

namespace Xtensive.Orm.Tests.Core.Modelling.IndexingModel
{
  /// <summary>
  /// References to included column.
  /// </summary>
  [Serializable]
  public sealed class IncludedColumnRef : ColumnInfoRef<SecondaryIndexInfo>
  {
    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<IncludedColumnRef, SecondaryIndexInfo, IncludedColumnRefCollection>(
        this, "IncludedColumns");
    }


    // Constructors

    /// <inheritdoc/>
    public IncludedColumnRef(SecondaryIndexInfo parent)
      : base(parent)
    {
    }

    public IncludedColumnRef(SecondaryIndexInfo parent, ColumnInfo column)
      : base(parent, column)
    {
    }
  }
}
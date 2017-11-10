// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.20

using System;
using Xtensive.Modelling;

namespace Xtensive.Orm.Tests.Core.Modelling.IndexingModel
{
  /// <summary>
  /// References to foreign key column.
  /// </summary>
  [Serializable]
  public sealed class ForeignKeyColumnRef : Ref<ColumnInfo, ForeignKeyInfo>
  {
    protected override Nesting CreateNesting()
    {
      return new Nesting<ForeignKeyColumnRef, ForeignKeyInfo, ForeignKeyColumnCollection>(
          this, "ForeignKeyColumns");
    }


    // Constructor

    public ForeignKeyColumnRef(ForeignKeyInfo parent)
      : base(parent)
    {
    }

    public ForeignKeyColumnRef(ForeignKeyInfo parent, ColumnInfo column)
      : base(parent)
    {
      Value = column;
    }
  }
}
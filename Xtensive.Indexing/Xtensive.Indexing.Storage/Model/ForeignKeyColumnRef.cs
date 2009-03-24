// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.24

using System;
using System.Diagnostics;
using Xtensive.Modelling;

namespace Xtensive.Indexing.Storage.Model
{
  [Serializable]
  public class ForeignKeyColumnRef: ColumnInfoRef<ForeignKeyInfo>
  {

    protected override Nesting CreateNesting()
    {
      return new Nesting<ForeignKeyColumnRef, ForeignKeyInfo, ColumnInfoRefCollection<ForeignKeyInfo>>(this, "KeyColumns");
    }


    public ForeignKeyColumnRef(ForeignKeyInfo parent, ColumnInfo column, int index)
      : base(parent, column, index)
    {
    }
  }
}
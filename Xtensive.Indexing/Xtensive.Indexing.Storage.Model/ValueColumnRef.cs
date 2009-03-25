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
  public class ValueColumnRef : ColumnInfoRef
  {
    protected override Nesting CreateNesting()
    {
      return new Nesting<ValueColumnRef, IndexInfo, ValueColumnRefCollection>(this, "ValueColumns");
    }


    // Constructors

    /// <inheritdoc/>
    protected ValueColumnRef(IndexInfo parent, int index)
      : base(parent, index)
    {
    }

    public ValueColumnRef(IndexInfo parent, ColumnInfo column, int index)
      : base(parent, column, index)
    {
    }
  }
}
// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using Xtensive.Modelling;
using System.Diagnostics;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// Describes a reference to value column in primary index.
  /// </summary>
  [Serializable]
  public class PrimaryValueColumnRef: ColumnInfoRef<PrimaryIndexInfo>
  {
    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<PrimaryValueColumnRef, PrimaryIndexInfo, ColumnInfoRefCollection<PrimaryIndexInfo>>(this, "ValueColumns");
    }


    //Constructors

    public PrimaryValueColumnRef(PrimaryIndexInfo primaryIndex, ColumnInfo column, int index)
      : base(primaryIndex, column, index)
    {
    }

  }
}
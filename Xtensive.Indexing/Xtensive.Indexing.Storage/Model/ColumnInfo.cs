// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using Xtensive.Modelling;
using System.Diagnostics;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// Describes a single column.
  /// </summary>
  [Serializable]
  public class ColumnInfo: NodeBase<PrimaryIndexInfo>
  {
    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<ColumnInfo, PrimaryIndexInfo, ColumnInfoCollection>(this, "Columns");
    }


    //Constructors

    public ColumnInfo(PrimaryIndexInfo index, string name)
      : base(index, name)
    {
    }

  }
}
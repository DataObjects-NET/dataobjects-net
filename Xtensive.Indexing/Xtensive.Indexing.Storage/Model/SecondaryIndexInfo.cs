// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using System.Diagnostics;
using Xtensive.Modelling;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// Describes a single secondary index.
  /// </summary>
  [Serializable]
  public class SecondaryIndexInfo : NodeBase<PrimaryIndexInfo>
  {

    /// <summary>
    /// Gets or sets the secondary key columns.
    /// </summary>
    [Property]
    public ColumnInfoRefCollection<SecondaryIndexInfo> SecondaryKeyColumns { get; private set; }

    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<SecondaryIndexInfo, PrimaryIndexInfo, SecondaryIndexInfoCollection>(this, "SecondaryIndexes");
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();
      SecondaryKeyColumns = new ColumnInfoRefCollection<SecondaryIndexInfo>(this, "SecondaryKeyColumns");
    }


    //Constructors

    public SecondaryIndexInfo(PrimaryIndexInfo primaryIndex, string name)
      : base(primaryIndex, name)
    {
    }

  }
}
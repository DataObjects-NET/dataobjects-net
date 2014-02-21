// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.01.14

using System;
using Xtensive.Modelling;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Orm.Upgrade.Model
{
  /// <summary>
  /// Reference to full-text column.
  /// </summary>
  [Serializable]
  public sealed class FullTextColumnRef : ColumnInfoRef<StorageFullTextIndexInfo>
  {
    [Property(Priority = -1100, CaseInsensitiveComparison = true)]
    public string Configuration { get; set; }

    [Property(Priority = -1200, CaseInsensitiveComparison = true)]
    public string TypeColumnName { get; set; } 

    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<FullTextColumnRef, StorageFullTextIndexInfo, FullTextColumnRefCollection>(this, "Columns");
    }


    // Constructors

    /// <inheritdoc/>
    public FullTextColumnRef(StorageFullTextIndexInfo parent)
      : base(parent)
    {}

    /// <inheritdoc/>
    public FullTextColumnRef(StorageFullTextIndexInfo parent, StorageColumnInfo column)
      : base(parent, column)
    {}

    /// <inheritdoc/>
    public FullTextColumnRef(StorageFullTextIndexInfo parent, StorageColumnInfo column, string configuration)
      : base(parent, column)
    {
      Configuration = configuration;
    }

    /// <inheritdoc/>
    public FullTextColumnRef(StorageFullTextIndexInfo parent, StorageColumnInfo column, string configuration, string typeColumn)
      : base(parent, column)
    {
      Configuration = configuration;
      TypeColumnName = typeColumn;
    }
  }
}
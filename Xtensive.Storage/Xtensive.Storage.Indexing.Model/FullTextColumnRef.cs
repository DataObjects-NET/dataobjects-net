// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.01.14

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Modelling;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Storage.Indexing.Model
{
  /// <summary>
  /// Reference to full-text column.
  /// </summary>
  [Serializable]
  public sealed class FullTextColumnRef : ColumnInfoRef<FullTextIndexInfo>
  {
    [Property(Priority = -1100)]
    public string Language { get; set; }

    [Property(Priority = -1200)]
    public ValueColumnRef TypeColumn { get; set; }

    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<FullTextColumnRef, FullTextIndexInfo, FullTextColumnRefCollection>(this, "Columns");
    }


    // Constructors

    /// <inheritdoc/>
    public FullTextColumnRef(FullTextIndexInfo parent)
      : base(parent)
    {}

    /// <inheritdoc/>
    public FullTextColumnRef(FullTextIndexInfo parent, ColumnInfo column)
      : base(parent, column)
    {}

    /// <inheritdoc/>
    public FullTextColumnRef(FullTextIndexInfo parent, ColumnInfo column, string language)
      : base(parent, column)
    {
      Language = language;
    }

    /// <inheritdoc/>
    public FullTextColumnRef(FullTextIndexInfo parent, ColumnInfo column, string language, ValueColumnRef typeColumn)
      : base(parent, column)
    {
      Language = language;
      TypeColumn = typeColumn;
    }
  }
}
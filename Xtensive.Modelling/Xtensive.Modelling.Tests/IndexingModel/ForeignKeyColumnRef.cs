// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.04.20

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Modelling.Tests.IndexingModel
{
  /// <summary>
  /// References to foreign key column.
  /// </summary>
  [Serializable]
  public sealed class ForeignKeyColumnRef : Ref<ColumnInfo, ForeignKeyInfo>
  {
    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<ForeignKeyColumnRef, ForeignKeyInfo, ForeignKeyColumnCollection>(
          this, "ForeignKeyColumns");
    }


    // Constructor

    /// <inheritdoc/>
    public ForeignKeyColumnRef(ForeignKeyInfo parent)
      : base(parent)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="parent">The foreign key.</param>
    /// <param name="column">The column.</param>
    /// <inheritdoc/>
    public ForeignKeyColumnRef(ForeignKeyInfo parent, ColumnInfo column)
      : base(parent)
    {
      Value = column;
    }
  }
}
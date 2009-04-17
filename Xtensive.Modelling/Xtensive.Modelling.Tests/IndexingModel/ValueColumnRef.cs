// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.24

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Modelling.Tests.IndexingModel
{
  /// <summary>
  /// References to value column.
  /// </summary>
  [Serializable]
  public sealed class ValueColumnRef : ColumnInfoRef<PrimaryIndexInfo>
  {
    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<ValueColumnRef, PrimaryIndexInfo, ValueColumnRefCollection>(this, "ValueColumns");
    }


    // Constructors

    /// <inheritdoc/>
    public ValueColumnRef(PrimaryIndexInfo parent)
      : base(parent)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="parent">The parent index.</param>
    /// <param name="column">The referenced column.</param>
    public ValueColumnRef(PrimaryIndexInfo parent, ColumnInfo column)
      : base(parent, column)
    {
    }
  }
}
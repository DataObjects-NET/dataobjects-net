// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.24

using System;
using Xtensive.Internals.DocTemplates;
using Xtensive.Modelling;

namespace Xtensive.Storage.Model
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
      return new Nesting<ValueColumnRef, PrimaryIndexInfo, ValueColumnRefCollection>(
        this, "ValueColumns");
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
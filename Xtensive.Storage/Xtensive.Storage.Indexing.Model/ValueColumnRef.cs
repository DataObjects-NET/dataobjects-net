// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.24

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Modelling;

namespace Xtensive.Storage.Indexing.Model
{
  /// <summary>
  /// References to value column.
  /// </summary>
  [Serializable]
  public class ValueColumnRef : ColumnInfoRef
  {
    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<ValueColumnRef, IndexInfo, ValueColumnRefCollection>(this, "ValueColumns");
    }


    // Constructors

    /// <inheritdoc/>
    public ValueColumnRef(IndexInfo parent, int index)
      : base(parent, index)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="parent">The parent index.</param>
    /// <param name="column">The referenced column.</param>
    /// <param name="index">The index.</param>
    public ValueColumnRef(IndexInfo parent, ColumnInfo column, int index)
      : base(parent, column, index)
    {
    }
  }
}
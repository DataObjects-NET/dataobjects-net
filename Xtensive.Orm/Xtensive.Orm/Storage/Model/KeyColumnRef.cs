// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Modelling;

namespace Xtensive.Storage.Model
{
  /// <summary>
  /// References to key column.
  /// </summary>
  [Serializable]
  public sealed class KeyColumnRef : KeyColumnRef<IndexInfo>
  {
    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<KeyColumnRef, IndexInfo, KeyColumnRefCollection>(this, "KeyColumns");
    }


    // Constructors

    /// <inheritdoc/>
    public KeyColumnRef(IndexInfo parent)
      : base(parent)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="parent">The parent index.</param>
    /// <param name="column">The referenced column.</param>
    public KeyColumnRef(IndexInfo parent, ColumnInfo column)
      : base(parent, column)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="parent">The parent index.</param>
    /// <param name="column">The referenced column.</param>
    /// <param name="direction">The direction.</param>
    public KeyColumnRef(IndexInfo parent, ColumnInfo column, Direction direction)
      : base(parent, column, direction)
    {
    }
  }
}
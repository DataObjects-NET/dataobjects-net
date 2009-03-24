// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Modelling;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// Describe a reference to key column in primary index.
  /// </summary>
  [Serializable]
  public class PrimaryKeyColumnRef : KeyColumnInfoRef<PrimaryIndexInfo>
  {
    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<PrimaryKeyColumnRef, PrimaryIndexInfo, ColumnInfoRefCollection<PrimaryIndexInfo>>(this, "KeyColumns");
    }


    // Constructors

    /// <inheritdoc/>
    public PrimaryKeyColumnRef(PrimaryIndexInfo primaryIndex, int index)
      : base(primaryIndex, index)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>.
    /// </summary>
    /// <param name="primaryIndex">The parent primary index.</param>
    /// <param name="column">The referenced column.</param>
    /// <param name="index">The index in columns collection.</param>
    /// <param name="direction">The direction.</param>
    public PrimaryKeyColumnRef(PrimaryIndexInfo primaryIndex, ColumnInfo column, int index, Direction direction)
      : base(primaryIndex, column, index, direction)
    {
    }


  }
}
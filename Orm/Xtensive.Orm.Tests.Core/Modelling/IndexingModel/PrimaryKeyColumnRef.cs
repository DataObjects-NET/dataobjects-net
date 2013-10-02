// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Modelling;

namespace Xtensive.Orm.Tests.Core.Modelling.IndexingModel
{
  /// <summary>
  /// References to key column.
  /// </summary>
  [Serializable]
  public sealed class PrimaryKeyColumnRef: KeyColumnRef<SecondaryIndexInfo>
  {
    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<PrimaryKeyColumnRef, SecondaryIndexInfo, PrimaryKeyColumnRefCollection>(this, "PrimaryKeyColumns");
    }


    // Constructors

    /// <inheritdoc/>
    public PrimaryKeyColumnRef(SecondaryIndexInfo parent)
      : base(parent)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="parent">The parent index.</param>
    /// <param name="column">The referenced column.</param>
    public PrimaryKeyColumnRef(SecondaryIndexInfo parent, ColumnInfo column)
      : base(parent, column)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="parent">The parent index.</param>
    /// <param name="column">The referenced column.</param>
    /// <param name="direction">The direction.</param>
    public PrimaryKeyColumnRef(SecondaryIndexInfo parent, ColumnInfo column, Direction direction)
      : base(parent, column, direction)
    {
    }
  }
}
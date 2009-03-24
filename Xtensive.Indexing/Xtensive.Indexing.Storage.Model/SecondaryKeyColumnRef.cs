// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Modelling;
using System.Diagnostics;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// Describes a reference to secondary key column in secondary index.
  /// </summary>
  [Serializable]
  public class SecondaryKeyColumnRef: KeyColumnInfoRef<SecondaryIndexInfo>
  {
    /// <inheritdoc/>
    protected override Nesting CreateNesting()
    {
      return new Nesting<SecondaryKeyColumnRef, SecondaryIndexInfo, ColumnInfoRefCollection<SecondaryIndexInfo>>(this, "SecondaryKeyColumns");
    }


    //Constructors

    /// <inheritdoc/>
    public SecondaryKeyColumnRef(SecondaryIndexInfo secondaryIndex, int index)
      : base(secondaryIndex, index)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="secondaryIndex">The parent secondary index.</param>
    /// <param name="column">The referenced column.</param>
    /// <param name="index">The index in columns collection.</param>
    /// <param name="direction">The direction.</param>
    public SecondaryKeyColumnRef(SecondaryIndexInfo secondaryIndex, ColumnInfo column, int index, Direction direction)
      : base(secondaryIndex, column, index, direction)
    {
    }

  }
}
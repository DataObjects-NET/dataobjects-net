// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using Xtensive.Modelling;
using System.Diagnostics;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// A base abstract class for all references to key column.
  /// </summary>
  /// <typeparam name="TParent">The type of the parent node.</typeparam>
  [Serializable]
  public abstract class KeyColumnInfoRef<TParent>: ColumnInfoRef<TParent>
    where TParent : Node
  {
    /// <summary>
    /// Gets or sets the column direction.
    /// </summary>
    [Property]
    public ColumnDirection Direction { get; set; }


    //Constructors

    protected KeyColumnInfoRef(TParent parent, ColumnInfo column, int index, ColumnDirection direction)
      : base(parent, column, index)
    {
      Direction = direction;
    }
  }
}
// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.20

using System;
using Xtensive.Core;
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
  public class KeyColumnRef: ColumnInfoRef
  {
    private Direction direction;

    /// <summary>
    /// Gets or sets the column direction.
    /// </summary>
    [Property]
    public Direction Direction
    {
      get { return direction; }
      set
      {
        EnsureIsEditable();
        using (var scope = LogPropertyChange("Direction", value))
        {
          direction = value;
          scope.Commit();
        }
      }
    }

    protected override Nesting CreateNesting()
    {
      return new Nesting<KeyColumnRef, IndexInfo, KeyColumnRefCollection>(this, "KeyColumns");
    }


    //Constructors

    protected KeyColumnRef(IndexInfo parent, int index)
      : base(parent, index)
    {
    }

    public KeyColumnRef(IndexInfo parent, ColumnInfo column, int index, Direction direction)
      : base(parent, column, index)
    {
      Direction = direction;
    }
  }
}
// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.16

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// Describes a column.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("{Name}; Type = {Type}")]
  public class ColumnInfo: Node<ColumnInfo, IndexInfo>
  {
    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    /// <value>The type.</value>
    public ColumnType Type
    {
      [DebuggerStepThrough]
      get; 
      [DebuggerStepThrough]
      private set;
    }

    /// <summary>
    /// Gets or sets the direction.
    /// </summary>
    /// <value>The direction.</value>
    public Direction Direction
    {
      [DebuggerStepThrough]
      get; 
      [DebuggerStepThrough]
      private set;
    }

    /// <inheritdoc/>
    protected override NodeCollection<ColumnInfo, IndexInfo> GetParentNodeCollection()
    {
      return Parent==null ? null : Parent.Columns;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="name">The name.</param>
    /// <param name="type">The type.</param>
    /// <param name="direction">The direction.</param>
    public ColumnInfo(IndexInfo index, string name, ColumnType type, Direction direction)
      :base(index, name)
    {
      Type = type;
      Direction = direction;
    }
  }
}
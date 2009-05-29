// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.05.26

using System;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage
{
  /// <summary>
  /// Marks persistent property as a part of primary key.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  public sealed class KeyFieldAttribute : StorageAttribute
  {
    /// <summary>
    /// Gets or sets the position of persistent property inside primary key.
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Gets or sets the sort direction. Default is <see cref="Core.Direction.Positive"/>.
    /// </summary>
    public Direction Direction { get; set; }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public KeyFieldAttribute()
      : this(0)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="position">The position of persistent property inside primary key.</param>
    public KeyFieldAttribute(int position)
      : this(position, Direction.Positive)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="direction">The sort direction.</param>
    public KeyFieldAttribute(Direction direction)
      : this(0, direction)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="position">The position of persistent property inside primary key.</param>
    /// <param name="direction">The sort direction.</param>
    public KeyFieldAttribute(int position, Direction direction)
    {
      Position = position;
      Direction = direction;
    }
  }
}
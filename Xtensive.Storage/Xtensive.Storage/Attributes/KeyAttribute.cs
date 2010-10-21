// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.05.26

using System;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Storage
{
  /// <summary>
  /// Marks persistent property as a part of primary key and 
  /// specifies <see cref="Position"/> and <see cref="Direction"/> of the field in key.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
  public sealed class KeyAttribute : StorageAttribute
  {
    /// <summary>
    /// Gets or sets the position of persistent property inside primary key.
    /// </summary>
    /// <remarks>
    /// <para>Each key in hierarchy should have an unique position from 0 to N-1 where N is number of key fields.</para>
    /// <para>
    /// Key fields positions and <see cref="Direction">directions</see> choice can be based on some optimization purposes, 
    /// e.g. sometimes it can be better to have specific order and <see cref="Direction"/> of key fields.
    /// </para>
    /// <para>Default position is <c>0</c>.</para>
    /// </remarks>
    public int Position { get; set; }

    /// <summary>
    /// Gets or sets the sort direction. Default is <see cref="Core.Direction.Positive"/>.
    /// </summary>
    /// <para>
    /// Key fields <see cref="Position">positions</see> and directions choice can be based on some optimization purposes, 
    /// e.g. sometimes it can be better to have specific order and direction of key fields.
    /// </para>
    public Direction Direction { get; set; }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public KeyAttribute()
      : this(0)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="position">The <see cref="Position">position</see> of persistent property inside primary key.</param>
    public KeyAttribute(int position)
      : this(position, Direction.Positive)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="direction">The <see cref="Direction">sort direction</see>.</param>
    public KeyAttribute(Direction direction)
      : this(0, direction)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="position">The <see cref="Position">position</see> of persistent property inside primary key.</param>
    /// <param name="direction">The <see cref="Direction">sort direction</see>.</param>
    public KeyAttribute(int position, Direction direction)
    {
      Position = position;
      Direction = direction;
    }
  }
}
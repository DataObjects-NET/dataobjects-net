// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.12.28

using System;
using Xtensive.Core;


namespace Xtensive.Orm.Model
{
  /// <summary>
  /// Describes a field that is a part of a primary key.
  /// </summary>
  [Serializable]
  public sealed class KeyField : Node
  {
    /// <summary>
    /// Gets or sets the direction.
    /// </summary>
    public Direction Direction { get; private set; }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return Name.GetHashCode() ^ Direction.GetHashCode();
    }

    /// <inheritdoc/>
    public override bool Equals(object obj) =>
      ReferenceEquals(this, obj)
      || obj is KeyField kf && Name == kf.Name && Direction == kf.Direction;


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="name">The name.</param>
    public KeyField(string name)
      : this(name, Direction.Positive)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyField"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="direction">The direction.</param>
    public KeyField(string name, Direction direction) : base(name)
    {
      Direction = direction;
    }
  }
}
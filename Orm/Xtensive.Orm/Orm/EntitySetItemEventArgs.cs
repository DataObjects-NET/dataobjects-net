// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.23

using System;
using System.Diagnostics;


namespace Xtensive.Orm
{
  /// <summary>
  /// Describes an event related to <see cref="EntitySet{TItem}"/> item.
  /// </summary>
  public class EntitySetItemEventArgs : EntitySetEventArgs
  {
    /// <summary>
    /// Gets the item to which this event is related.
    /// </summary>
    public Entity Item { get; private set; }


    // Cosntructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="entitySet">The entity set.</param>
    /// <param name="item">The item.</param>
    public EntitySetItemEventArgs(EntitySetBase entitySet, Entity item)
      : base(entitySet)
    {
      Item = item;
    }
  }
}
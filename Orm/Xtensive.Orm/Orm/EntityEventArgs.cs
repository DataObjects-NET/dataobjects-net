// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.06.04

using System;


namespace Xtensive.Orm
{
  /// <summary>
  /// Describes <see cref="Entity"/>-related events.
  /// </summary>
  public class EntityEventArgs : EventArgs
  {
    /// <summary>
    /// Gets the entity to which this event is related.
    /// </summary>
    public Entity Entity { get; private set; }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="entity">The entity.</param>
    public EntityEventArgs(Entity entity)
    {
      Entity = entity;
    }
  }
}
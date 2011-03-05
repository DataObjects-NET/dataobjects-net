// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.06.04

using System;
using Xtensive.Internals.DocTemplates;

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
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="entity">The entity.</param>
    public EntityEventArgs(Entity entity)
    {
      Entity = entity;
    }
  }
}
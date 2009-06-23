// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.06.04

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage
{
  /// <summary>
  /// Arguments for <see cref="Entity"/>-related events.
  /// </summary>
  public class EntityEventArgs : EventArgs
  {
    /// <summary>
    /// Gets the entity.
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
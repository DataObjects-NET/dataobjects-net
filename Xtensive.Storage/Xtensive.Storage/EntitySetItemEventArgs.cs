// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.23

using System;
using System.Diagnostics;

namespace Xtensive.Storage
{
  /// <summary>
  /// Arguments for <see cref="EntitySetBase"/>-related events.
  /// </summary>
  public class EntitySetItemEventArgs : EntityEventArgs
  {
    /// <summary>
    /// Gets the entity set.
    /// </summary>
    /// <value>The entity set.</value>
    public EntitySetBase EntitySet { get; private set; }


    // Constructors

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="entitySet">The entity set.</param>
    public EntitySetItemEventArgs(Entity entity, EntitySetBase entitySet)
      : base(entity)
    {
      EntitySet = entitySet;
    }
  }
}
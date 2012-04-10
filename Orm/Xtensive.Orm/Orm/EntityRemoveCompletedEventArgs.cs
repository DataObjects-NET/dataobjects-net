// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;

namespace Xtensive.Orm
{
  /// <summary>
  /// Arguments for completing entity remove event.
  /// </summary>
  public class EntityRemoveCompletedEventArgs : EntityEventArgs
  {
    /// <summary>
    /// Gets the exception.
    /// </summary>
    /// <value>The exception.</value>
    public Exception Exception { get; private set; }


    // Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityRemoveCompletedEventArgs"/> class.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="exception">The exception.</param>
    public EntityRemoveCompletedEventArgs(Entity entity, Exception exception)
      : base(entity)
    {
      Exception = exception;
    }
  }
}
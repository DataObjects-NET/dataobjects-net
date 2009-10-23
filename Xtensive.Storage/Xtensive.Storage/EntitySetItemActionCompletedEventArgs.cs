// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.23

using System;
using System.Diagnostics;

namespace Xtensive.Storage
{
  public class EntitySetItemActionCompletedEventArgs : EntitySetItemEventArgs
  {
    public Exception Exception { get; private set; }


    // Cosntructors

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="entitySet">The entity set.</param>
    /// <param name="exception">The ><see cref="Exception"/> property initial value.</param>
    public EntitySetItemActionCompletedEventArgs(Entity entity, EntitySetBase entitySet, Exception exception)
      : base(entity, entitySet)
    {
      Exception = exception;
    }
  }
}
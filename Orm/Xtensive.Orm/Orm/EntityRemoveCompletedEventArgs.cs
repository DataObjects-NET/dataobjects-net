// Copyright (C) 2003-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
    public Exception Exception { get; }


    // Constructors

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="exception">The <see cref="Exception"/> initial value.</param>
    public EntityRemoveCompletedEventArgs(Entity entity, Exception exception)
      : base(entity)
    {
      Exception = exception;
    }
  }
}
// Copyright (C) 2009-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;

using Xtensive.Orm.Model;

namespace Xtensive.Orm
{
  /// <summary>
  /// Describes <see cref="Entity"/> field set completion events.
  /// </summary>
  public class EntityFieldValueSetCompletedEventArgs : EntityFieldValueSetEventArgs
  {
    /// <summary>
    /// Gets the exception, if any, that was thrown on setting the field value.
    /// </summary>
    public Exception Exception { get; private set; }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="field">The field.</param>
    /// <param name="oldValue">The <see cref="EntityFieldValueSetEventArgs.OldValue"/> value.</param>
    /// <param name="newValue">The <see cref="EntityFieldValueSetEventArgs.NewValue"/> value.</param>
    /// <param name="exception">The <see cref="Exception"/> value.</param>
    public EntityFieldValueSetCompletedEventArgs(Entity entity, FieldInfo field, object oldValue, object newValue, Exception exception)
      : base(entity, field, oldValue, newValue)
    {
      Exception = exception;
    }
  }
}
// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using Xtensive.Internals.DocTemplates;
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
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
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
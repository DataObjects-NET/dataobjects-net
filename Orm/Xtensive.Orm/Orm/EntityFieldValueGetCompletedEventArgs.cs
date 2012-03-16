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
  /// Describes <see cref="Entity"/> field reading completion events.
  /// </summary>
  public class EntityFieldValueGetCompletedEventArgs : EntityFieldValueEventArgs
  {
    /// <summary>
    /// Gets the exception, if any, that was thrown on getting the field value.
    /// </summary>
    public Exception Exception { get; private set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="field">The field.</param>
    /// <param name="value">The <see cref="EntityFieldValueEventArgs.Value"/> value.</param>
    /// <param name="exception">The <see cref="Exception"/> value.</param>
    public EntityFieldValueGetCompletedEventArgs(Entity entity, FieldInfo field, object value, Exception exception)
      : base(entity, field, value)
    {
      Exception = exception;
    }
  }
}
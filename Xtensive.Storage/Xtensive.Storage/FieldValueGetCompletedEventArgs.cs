// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  /// <summary>
  /// Arguments for completing get value event.
  /// </summary>
  public class FieldValueGetCompletedEventArgs : FieldValueEventArgs
  {
    /// <summary>
    /// Gets the exception.
    /// </summary>
    public Exception Exception { get; private set; }


    // Constructors

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="field">The field.</param>
    /// <param name="value">The <see cref="FieldValueEventArgs.Value"/> property initial value.</param>
    /// <param name="exception">The <see cref="Exception"/> property initial value.</param>
    public FieldValueGetCompletedEventArgs(Entity entity, FieldInfo field, object value, Exception exception)
      : base(entity, field, value)
    {
      Exception = exception;
    }
  }
}
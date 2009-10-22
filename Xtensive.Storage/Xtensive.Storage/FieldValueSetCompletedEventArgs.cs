// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using System.Diagnostics;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  /// <summary>
  /// Arguments for completing set value event.
  /// </summary>
  public class FieldValueSetCompletedEventArgs : FieldValueSetEventArgs
  {
    /// <summary>
    /// Gets the exception.
    /// </summary>
    /// <value>The exception.</value>
    public Exception Exception { get; private set; }


    // Constructors

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="field">The field.</param>
    /// <param name="oldValue">The <see cref="FieldValueSetEventArgs.OldValue"/> initial value.</param>
    /// <param name="newValue">The <see cref="FieldValueSetEventArgs.NewValue"/> initial value.</param>
    /// <param name="exception">The <see cref="Exception"/> property initial value.</param>
    public FieldValueSetCompletedEventArgs(Entity entity, FieldInfo field, object oldValue, object newValue, Exception exception)
      : base(entity, field, oldValue, newValue)
    {
      Exception = exception;
    }
  }
}
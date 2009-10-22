// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.10.08

using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  /// <summary>
  /// Arguments for field-related events.
  /// </summary>
  public class FieldValueEventArgs : FieldEventArgs
  {
    /// <summary>
    /// Gets the field value.
    /// </summary>
    public object Value { get; private set; }


    // Constructors

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="field">The field.</param>
    /// <param name="value">The <see cref="Value"/> property initial value.</param>
    public FieldValueEventArgs(Entity entity, FieldInfo field, object value)
      : base(entity, field)
    {
      Value = value;
    }
  }
}
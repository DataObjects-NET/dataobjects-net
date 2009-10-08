// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.10.08

using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  /// <summary>
  /// Field-related event arguments containing old and new field values.
  /// </summary>
  public class ChangeFieldValueEventArgs : FieldEventArgs
  {
    /// <summary>
    /// Gets the old value.
    /// </summary>
    /// <value>The old value.</value>
    public object OldValue { get; private set; }

    /// <summary>
    /// Gets the new value.
    /// </summary>
    public object NewValue { get; private set; }


    // Constructors

    public ChangeFieldValueEventArgs(Entity entity, FieldInfo field, object oldValue, object newValue)
      : base(entity, field)
    {
      OldValue = oldValue;
      NewValue = newValue;
    }
  }
}
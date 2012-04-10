// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.10.08


using Xtensive.Orm.Model;

namespace Xtensive.Orm
{
  /// <summary>
  /// Describes <see cref="Entity"/> field-related events containing old and new field values.
  /// </summary>
  public class EntityFieldValueSetEventArgs : EntityFieldEventArgs
  {
    /// <summary>
    /// Gets the old value.
    /// </summary>
    public object OldValue { get; private set; }

    /// <summary>
    /// Gets the new value.
    /// </summary>
    public object NewValue { get; private set; }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="field">The field.</param>
    /// <param name="oldValue">The <see cref="OldValue"/> property value.</param>
    /// <param name="newValue">The <see cref="NewValue"/> property value.</param>
    public EntityFieldValueSetEventArgs(Entity entity, FieldInfo field, object oldValue, object newValue)
      : base(entity, field)
    {
      OldValue = oldValue;
      NewValue = newValue;
    }
  }
}
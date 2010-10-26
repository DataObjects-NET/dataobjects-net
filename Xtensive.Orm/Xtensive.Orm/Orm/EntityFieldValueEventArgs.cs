// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.10.08

using Xtensive.Internals.DocTemplates;
using Xtensive.Orm.Model;

namespace Xtensive.Orm
{
  /// <summary>
  /// Describes <see cref="Entity"/> field related events containing field value.
  /// </summary>
  public class EntityFieldValueEventArgs : EntityFieldEventArgs
  {
    /// <summary>
    /// Gets the field value.
    /// </summary>
    public object Value { get; private set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="field">The field.</param>
    /// <param name="value">The <see cref="Value"/> property value.</param>
    public EntityFieldValueEventArgs(Entity entity, FieldInfo field, object value)
      : base(entity, field)
    {
      Value = value;
    }
  }
}
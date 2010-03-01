// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.10.08

using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  /// <summary>
  /// Describes <see cref="Entity"/> field-related events.
  /// </summary>
  public class EntityFieldEventArgs : EntityEventArgs
  {
    /// <summary>
    /// Gets the field to which this event is related.
    /// </summary>
    public FieldInfo Field { get; private  set; }


    // Constructors


    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="field">The field.</param>
    public EntityFieldEventArgs(Entity entity, FieldInfo field)
      : base(entity)
    {
      Field = field;
    }
  }
}
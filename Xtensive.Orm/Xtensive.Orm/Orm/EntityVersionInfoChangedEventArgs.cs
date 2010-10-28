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
  /// Describes <see cref="Entity"/>.<see cref="Entity.VersionInfo"/> change-related events.
  /// </summary>
  public class EntityVersionInfoChangedEventArgs : EntityFieldEventArgs
  {
    /// <summary>
    /// Gets or sets a value indicating whether 
    /// <see cref="Entity.VersionInfo"/> was changed or not.
    /// </summary>
    public bool Changed { get; private set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="changedEntity">The entity that was changed.</param>
    /// <param name="changedField">The field that was changed.</param>
    /// <param name="changed"><see cref="Changed"/> property value.</param>
    public EntityVersionInfoChangedEventArgs(Entity changedEntity, FieldInfo changedField, bool changed)
      : base(changedEntity, changedField)
    {
      Changed = changed;
    }
  }
}
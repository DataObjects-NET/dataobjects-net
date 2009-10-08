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
  public class FieldEventArgs : EntityEventArgs
  {
    /// <summary>
    /// Gets the field.
    /// </summary>
    public FieldInfo Field { get; private  set; }


    // Constructors

    public FieldEventArgs(Entity entity, FieldInfo field)
      : base(entity)
    {
      Field = field;
    }
  }
}
// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.01.24

namespace Xtensive.ObjectMapping.Model
{
  /// <summary>
  /// Kind of an object whose type described by a <see cref="TypeDescription"/>.
  /// </summary>
  public enum ObjectKind
  {
    /// <summary>
    /// The entity.
    /// </summary>
    Entity,

    /// <summary>
    /// The user structure.
    /// </summary>
    UserStructure,

    /// <summary>
    /// The primitive type.
    /// </summary>
    Primitive
  }
}
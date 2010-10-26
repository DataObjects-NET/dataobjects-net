// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitry Maximov
// Created:    2007.09.29

using System;

namespace Xtensive.Orm.Model
{
  /// <summary>
  /// Type attributes.
  /// </summary>
  [Flags]
  public enum TypeAttributes
  {
    None = 0,
    /// <summary>
    /// Type is entity.
    /// </summary>
    Entity = 0x1,
    /// <summary>
    /// Type is structure.
    /// </summary>
    Structure = 0x4,
    /// <summary>
    /// Type is persistent interface.
    /// </summary>
    Interface = 0x8,
    /// <summary>
    /// Type is abstract.
    /// </summary>
    Abstract = 0x10,
    /// <summary>
    /// Type is materialized interface.
    /// </summary>
    Materialized = 0x20,
    /// <summary>
    /// Type is system.
    /// </summary>
    System = 0x40,
    /// <summary>
    /// Type is auxilary (entity set item).
    /// </summary>
    Auxiliary = 0x80,
    /// <summary>
    /// Type is generic type definition.
    /// </summary>
    GenericTypeDefinition = 0x100,
    /// <summary>
    /// Type is automatically registered generic type instance.
    /// </summary>
    AutoGenericInstance = 0x200,
  }
}
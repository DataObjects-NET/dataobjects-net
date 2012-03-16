// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2007.09.29

using System;

namespace Xtensive.Orm.Model
{
  /// <summary>
  /// Field attributes.
  /// </summary>
  [Flags]
  public enum FieldAttributes
  {
    None = 0,
    Nullable = 0x1,
    Collatable = 0x2,
    Translatable = 0x4,
    Structure = 0x8,
    Declared = 0x10,
    Inherited = 0x20,
    Entity = 0x40,
    EntitySet = 0x80,
    PrimaryKey = 0x0100,
    NonPrimitive = Entity | EntitySet | Structure,
    System = 0x0400,
    LazyLoad = 0x0800,
    InterfaceImplementation = 0x1000,
    Explicit = 0x2000,
    Enum = 0x4000,
    TypeId = 0x8000,
    SkipVersion = 0x20000,
    ManualVersion = 0x40000,
    AutoVersion = 0x80000,
    TypeDiscriminator = 0x100000,
    Computed = 0x200000,
    Indexed = 0x400000,
    Nested = 0x800000,
    NotIndexed = 0x1000000,
  }
}
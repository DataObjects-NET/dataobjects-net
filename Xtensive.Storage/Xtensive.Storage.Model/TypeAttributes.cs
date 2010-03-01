// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitry Maximov
// Created:    2007.09.29

using System;

namespace Xtensive.Storage.Model
{
  [Flags]
  public enum TypeAttributes
  {
    None = 0,
    Entity = 0x1,
    Structure = 0x4,
    Interface = 0x8,
    Abstract = 0x10,
    Materialized = 0x20,
    System = 0x40,
    GenericTypeDefinition = 0x80,
    AuxiliaryType = 0x100
  }
}
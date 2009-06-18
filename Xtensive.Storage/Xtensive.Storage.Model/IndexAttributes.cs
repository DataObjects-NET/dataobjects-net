// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.07.23

using System;

namespace Xtensive.Storage.Model
{
  [Flags]
  public enum IndexAttributes
  {
    None = 0x0,
    Primary = 0x1,
    Unique = 0x2,
    FullText = 0x4,
    Real = 0x8,
    Virtual = 0x10,
    Union = 0x20,
    Join = 0x40,
    Filtered = 0x80,
    Secondary = 0x100,
    Abstract = 0x200
  }
}
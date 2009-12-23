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
    None =        0x0,
    Primary =     0x1,
    Unique =      0x2,
    Real =        0x8,
    Secondary =  0x10,
    Abstract =   0x20,
    Virtual =    0x40,
    Typed =      0x80,
    Union =     0x100,
    Join =      0x200,
    Filtered =  0x400,
    View =      0x800
  }
}
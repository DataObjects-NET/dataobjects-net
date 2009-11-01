// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.09

using System;

namespace Xtensive.Core.Testing
{
  [Flags]
  public enum SeedVariatorType
  {
    Default = 0x0,
    None = 0x0,
    CallingType = 0x1,
    CallingMethod = 0x2,
    CallingAssembly = 0x4,
    Day = 0x8,
  }
}
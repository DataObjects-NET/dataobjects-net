// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.28

using System;

namespace Xtensive.Integrity.Atomicity
{
  /// <summary>
  /// <see cref="AtomicityContextBase"/> options.
  /// </summary>
  [Flags]
  public enum AtomicityContextOptions
  {
    Default  = 0x1,
    None     = 0x0,
    Undoable = 0x1,
    Redoable = 0x2,
    Validate = 0x4,
    Full     = 0x7,
  }
}
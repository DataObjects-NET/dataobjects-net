// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.18

using System;

namespace Xtensive.Orm
{
  /// <summary>
  /// Possible persistent field states.
  /// </summary>
  public enum PersistentFieldState
  {
    /// <summary>
    /// Field value is loaded, so an attempt to read it won't lead to database roundptrip.
    /// </summary>
    Loaded = 0x1,
    /// <summary>
    /// Field value is loaded and modified, but not yet persisted.
    /// </summary>
    Modified = 0x2,
  }
}
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
    /// <summary>
    /// Default value. Actualy Undoable.
    /// </summary>
    Default  = Undoable,

    /// <summary>
    /// Atomicity features are not supportet.
    /// </summary>
    None     = 0x0,

    /// <summary>
    /// Undo operation is supported.
    /// </summary>
    Undoable = 0x1,

    /// <summary>
    /// Redo operation is supported.
    /// </summary>
    Redoable = 0x2,

    /// <summary>
    /// Validation is supported.
    /// </summary>
    Validate = 0x4,

    /// <summary>
    /// All atomicity features are supported.
    /// </summary>
    Full     = Undoable | Redoable | Validate
  }
}
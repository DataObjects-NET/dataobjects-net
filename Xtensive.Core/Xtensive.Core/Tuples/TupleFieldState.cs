// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.06.18

using System;

namespace Xtensive.Tuples
{
  /// <summary>
  /// Defines field state that can be set or get for each field in <see cref="Tuple"/>.
  /// </summary>
  [Flags]
  public enum TupleFieldState
  {
    /// <summary>
    /// Default value
    /// </summary>
    Default = 0x0,

    /// <summary>
    /// Field value is available in tuple.
    /// </summary>
    Available = 0x01,

    /// <summary>
    /// Field has null value.
    /// Used with both nullable and non-nullable fields.
    /// </summary>
    Null = 0x02,
  }
}
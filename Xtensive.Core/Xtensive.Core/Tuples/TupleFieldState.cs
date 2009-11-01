// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.06.18

using System;

namespace Xtensive.Core.Tuples
{
  /// <summary>
  /// Tuple field state enumeration.
  /// Defines field state that can be set or get
  /// for each field in <see cref="Tuple"/>.
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
    IsAvailable = 0x01,

    /// <summary>
    /// Field has null value.
    /// Used with both nullable and non-nullable fields.
    /// </summary>
    IsNull = 0x02,
  }
}
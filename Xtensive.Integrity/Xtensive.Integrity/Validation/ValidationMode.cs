// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.21

using System;

namespace Xtensive.Integrity.Validation
{
  /// <summary>
  /// Possible validation modes.
  /// </summary>
  [Serializable]
  public enum ValidationMode
  {
    /// <summary>
    /// The same as <see cref="ImmediateOrDelayed"/>.
    /// </summary>
    Default = ImmediateOrDelayed,
    /// <summary>
    /// Unknown validation mode.
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// Immediate valdation.
    /// </summary>
    Immediate = 1,
    /// <summary>
    /// Delayed valdation.
    /// </summary>
    Delayed = 2,
    /// <summary>
    /// Immediate or delayed valdation.
    /// </summary>
    ImmediateOrDelayed = 3,
  }
}
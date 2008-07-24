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
    /// Immediate or delayed (if validation context is in inconsistent state) valdation.
    /// </summary>
    ImmediateOrDelayed = 0,

    /// <summary>
    /// Immediate valdation.
    /// </summary>
    Immediate = 1
  }
}
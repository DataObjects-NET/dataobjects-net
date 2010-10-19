// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.10

using System;

namespace Xtensive.Arithmetic
{
  /// <summary>
  /// Describes how arithmetics treats <see langword="null"/> in operations.   
  /// </summary>
  [Serializable]
  public enum NullBehavior : sbyte
  {
    /// <summary>
    /// Default <see langword="null"/> behavior.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Treats <see langword="null"/> as <see langword="zero"/> in additions and subtractions.
    /// </summary>
    ThreatNullAsZero = 0,

    /// <summary>
    /// Treats <see langword="null"/> as is in additions and subtractions. If one of parameters is null, the result will be always null.
    /// </summary>
    ThreatNullAsNull = 1,
  }
}
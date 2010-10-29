// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.10

using System;

namespace Xtensive.Arithmetic
{
  /// <summary>
  /// Describes arithmetic overflow behavior.   
  /// </summary>
  [Serializable]
  public enum OverflowBehavior : sbyte
  {
    /// <summary>
    /// Default overflow behaviour.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Overflow is suppressed.
    /// </summary>
    DenyOverflow = 0,

    /// <summary>
    /// Overflow is allowed.
    /// </summary>
    AllowOverflow = 1,
  }
}
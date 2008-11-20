// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.17

using Xtensive.Core.Aspects.Helpers;

namespace Xtensive.Core.Aspects
{
  /// <summary>
  /// Priorities of key aspects.
  /// </summary>
  public enum CoreAspectPriority
  {
    /// <summary>
    /// <see cref="TraceAttribute"/> aspect priority.
    /// Value is <see langword="-1000000" />.
    /// </summary>
    Trace = -1000000,
    /// <summary>
    /// <see cref="ChangerAttribute"/> aspect priority.
    /// Value is <see langword="1000000" />.
    /// </summary>
    Changer =  1000000,
  }
}
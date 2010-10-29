// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.09.10

using System;

namespace Xtensive.Core
{
  /// <summary>
  /// Possible directions of iteration through the enumerable.
  /// </summary>
  [Serializable]
  public enum Direction : sbyte
  {
    /// <summary>
    /// Default direction (none).
    /// </summary>
    Default = 0,

    /// <summary>
    /// No direction.
    /// Implies that either comparison for it can't be performed, or shouldn't be done.
    /// </summary>
    None = 0,

    /// <summary>
    /// Forward direction (acsending order).
    /// </summary>
    Positive = 1,

    /// <summary>
    /// Backward direction (descending order).
    /// </summary>
    Negative = -1,
  }
}
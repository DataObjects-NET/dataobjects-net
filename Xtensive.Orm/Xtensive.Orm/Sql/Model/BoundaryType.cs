// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql
{
  /// <summary>
  /// Specifies to which side of each boundary value interval, left or right, the boundary value
  /// belongs.
  /// </summary>
  [Serializable]
  public enum BoundaryType
  {
    /// <summary>
    /// Default value is equal to <see cref="Left"/>.
    /// </summary>
    Default = 0,
    /// <summary>
    /// Specifies that boundary value belongs to the left side of boundary value interval.
    /// </summary>
    Left = 0,
    /// <summary>
    /// Specifies that boundary value belongs to the right side of boundary value interval.
    /// </summary>
    Right = 1,
  }
}

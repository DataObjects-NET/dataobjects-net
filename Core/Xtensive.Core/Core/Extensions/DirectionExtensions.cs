// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.06.20

using Xtensive.Core;

namespace Xtensive.Core
{
  /// <summary>
  ///   <see cref="Direction"/> related extension methods.
  /// </summary>
  public static class DirectionExtensions
  {
    /// <summary>
    /// Inverts the specified direction.
    /// </summary>
    /// <param name="direction">The direction to invert.</param>
    /// <returns>Inverted direction.</returns>
    public static Direction Invert(this Direction direction)
    {
      return (Direction) (-(int) direction);
    }

    /// <summary>
    /// Combines the <paramref name="direction"/> with the <paramref name="combineWith"/>.
    /// In fact, multiplies integer values of both directions.
    /// </summary>
    /// <param name="direction">The original direction.</param>
    /// <param name="combineWith">The direction to combine with.</param>
    /// <returns>The result of combination.</returns>
    public static Direction Combine(this Direction direction, Direction combineWith)
    {
      switch (combineWith) {
      case Direction.Negative:
        return direction.Invert();
      case Direction.Positive:
        return direction;
      default:
        return Direction.None;
      }
    }
  }
}
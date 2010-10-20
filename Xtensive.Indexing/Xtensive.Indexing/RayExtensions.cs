// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.02.12

using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Indexing
{
  /// <summary>
  /// <see cref="Ray{T}"/> extension methods.
  /// </summary>
  public static class RayExtensions
  {
    #region Contains, Intersects methods

    /// <summary>
    /// Checks if this instance contains specified point.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Ray{T}"/> point.</typeparam>
    /// <param name="ray">Ray to check.</param>
    /// <param name="point">Point to check for containment.</param>
    /// <param name="comparer">The comparer to use.</param>
    /// <returns>
    ///   <see langword="True"/> if this instance contains the specified point; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool Contains<T>(this Ray<T> ray, T point, AdvancedComparer<T> comparer)
    {
      // ArgumentValidator.EnsureArgumentNotNull(comparer, "comparer");
      return
        ray.Direction == Direction.Positive
          ? comparer.Compare(ray.Point, point) <= 0
          : comparer.Compare(ray.Point, point) >= 0;
    }

    /// <summary>
    /// Checks if ray intersects with another ray (i.e. they have common part).
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Ray{T}"/> point.</typeparam>
    /// <param name="first">First ray to check.</param>
    /// <param name="second">Second ray to check.</param>
    /// <param name="comparer">The comparer to use.</param>
    /// <returns>
    ///   <see langword="True"/> if ray intersects with the specified ray; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool Intersects<T>(this Ray<T> first, Ray<T> second, AdvancedComparer<T> comparer)
    {
      // ArgumentValidator.EnsureArgumentNotNull(comparer, "comparer");
      if (first.Direction == second.Direction)
        return true;
      return Contains(first, second.Point, comparer);
    }

    #endregion

    #region CompareTo, EqualTo methods

    /// <summary>
    /// Compares this ray with <paramref name="second"/> ray.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="Ray{T}"/> point.</typeparam>
    /// <param name="first">The first ray to compare.</param>
    /// <param name="second">The ray to compare with the <paramref name="first"/> one.</param>
    /// <param name="comparer">The comparer to use.</param>
    /// <returns>
    /// Less than zero if <see cref="Ray{T}"/> less than<paramref name="second"/>;
    /// zero if <see cref="Ray{T}"/> equals to <paramref name="second"/>;
    /// +1 otherwise.
    /// </returns>
    public static int CompareTo<T>(this Ray<T> first, Ray<T> second, AdvancedComparer<T> comparer)
    {
      int result = comparer.Compare(first.Point, second.Point);

      if (result != 0)
        return result;

      // Вершины равны
      return first.Direction - second.Direction;
    }

    /// <summary>
    /// Compares the <see cref="Ray{T}"/> with <paramref name="second"/> item.
    /// </summary>
    /// <param name="first">The first ray to compare.</param>
    /// <param name="second">The ray to compare with the <paramref name="first"/> one.</param>
    /// <param name="comparer">The comparer.</param>
    /// <returns>
    /// True if the <see cref="Ray{T}"/> equals to <paramref name="second"/>;
    /// false otherwise.
    /// </returns>
    public static bool EqualTo<T>(this Ray<T> first, Ray<T> second, AdvancedComparer<T> comparer)
    {
      // ArgumentValidator.EnsureArgumentNotNull(comparer, "comparer");
      return first.Direction == second.Direction && comparer.Equals(first.Point, second.Point);
    }

    #endregion
  }
}
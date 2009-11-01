// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.02.12

using Xtensive.Core;
using Xtensive.Core.Comparison;

namespace Xtensive.Indexing
{
  public struct RayUtils<T>
  {
    private readonly AdvancedComparer<T> comparer;

    #region Static methods

    /// <summary>
    /// Checks if this instance contains specified point.
    /// </summary>
    /// <param name="point">Point to check for containment.</param>
    /// <param name="comparer">The comparer to use.</param>
    /// <returns>
    ///   <see langword="True"/> if this instance contains the specified point; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool Contains(Ray<T> ray, T point, AdvancedComparer<T> comparer)
    {
      ArgumentValidator.EnsureArgumentNotNull(comparer, "comparer");
      return
        ray.Direction == Direction.Positive
          ? comparer.Compare(ray.Point, point) <= 0
          : comparer.Compare(ray.Point, point) >= 0;
    }

    /// <summary>
    /// Checks if ray intersects with another ray (i.e. they have common part).
    /// </summary>
    /// <param name="other">Ray to check for intersection.</param>
    /// <param name="comparer">The comparer to use.</param>
    /// <returns>
    ///   <see langword="True"/> if ray intersects with the specified ray; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool Intersects(Ray<T> ray, Ray<T> other, AdvancedComparer<T> comparer)
    {
      ArgumentValidator.EnsureArgumentNotNull(comparer, "comparer");
      if (ray.Direction == other.Direction)
        return true;
      return Contains(ray, other.Point, comparer);
    }

    /// <summary>
    /// Compares this ray with <paramref name="other"/> ray.
    /// </summary>
    /// <param name="other">A <see cref="Ray{T}"/> to compare with.</param>
    /// <param name="comparer">The comparer to use.</param>
    /// <returns>
    /// Less than zero if <see cref="Ray{T}"/> less than<paramref name="other"/>;
    /// zero if <see cref="Ray{T}"/> equals to <paramref name="other"/>;
    /// +1 otherwise.
    /// </returns>
    public static int CompareTo(Ray<T> ray, Ray<T> other, AdvancedComparer<T> comparer)
    {
      int result = comparer.Compare(ray.Point, other.Point);

      if (result != 0)
        return result;

      // Вершины равны
      return ray.Direction - other.Direction;
    }

    /// <summary>
    /// Compares the <see cref="Ray{T}"/> with <paramref name="other"/> item.
    /// </summary>
    /// <param name="other">A <see cref="Ray{T}"/> to compare with.</param>
    /// <param name="comparer">The comparer.</param>
    /// <returns>
    /// True if the <see cref="Ray{T}"/> equals to <paramref name="other"/>;
    /// false otherwise.
    /// </returns>
    public static bool Equals(Ray<T> ray, Ray<T> other, AdvancedComparer<T> comparer)
    {
      ArgumentValidator.EnsureArgumentNotNull(comparer, "comparer");
      return ray.Direction == other.Direction && comparer.Equals(ray.Point, other.Point);
    }

    #endregion

    /// <summary>
    /// Checks if this instance contains specified point.
    /// </summary>
    /// <param name="point">Point to check for containment.</param>
    /// <returns>
    ///   <see langword="True"/> if this instance contains the specified point; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(Ray<T> ray, T point)
    {
      return Contains(ray, point, comparer);
    }

    /// <summary>
    /// Checks if ray intersects with another ray (i.e. they have common part).
    /// </summary>
    /// <param name="other">Ray to check for intersection.</param>
    /// <returns>
    ///   <see langword="True"/> if ray intersects with the specified ray; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Intersects(Ray<T> ray, Ray<T> other)
    {
      return Intersects(ray, other, comparer);
    }

    /// <summary>
    /// Compares this ray with <paramref name="other"/> ray.
    /// </summary>
    /// <param name="other">A <see cref="Ray{T}"/> to compare with.</param>
    /// <returns>
    /// Less than zero if <see cref="Ray{T}"/> less than<paramref name="other"/>;
    /// zero if <see cref="Ray{T}"/> equals to <paramref name="other"/>;
    /// +1 otherwise.
    /// </returns>
    public int CompareTo(Ray<T> ray, Ray<T> other)
    {
      return CompareTo(ray, other, comparer);
    }

    /// <summary>
    /// Compares the <see cref="Ray{T}"/> with <paramref name="other"/> item.
    /// </summary>
    /// <param name="other">A <see cref="Ray{T}"/> to compare with.</param>
    /// <returns>
    /// True if the <see cref="Ray{T}"/> equals to <paramref name="other"/>;
    /// false otherwise.
    /// </returns>
    public bool Equals(Ray<T> ray, Ray<T> other)
    {
      return Equals(ray, other, comparer);
    }


    // Constructors

    public RayUtils(AdvancedComparer<T> comparer)
    {
      this.comparer = comparer;
    }
  }
}
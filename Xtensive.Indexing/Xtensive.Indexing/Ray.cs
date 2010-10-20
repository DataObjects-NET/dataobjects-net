// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.09.06

using System;
using System.Diagnostics;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Ray on an ordered set of items of type <typeparamref name="T"/>.
  /// </summary>
  /// <typeparam name="T">The type of item.</typeparam>
  public struct Ray<T>
    : IEquatable<Ray<T>>,
      IComparable<Ray<T>>
  {
    private static readonly AdvancedComparer<T> comparer = AdvancedComparer<T>.System;
    private readonly Direction direction;
    private readonly T point;

    /// <summary>
    /// Gets the point of this instance.
    /// </summary>
    public T Point
    {
      [DebuggerStepThrough]
      get { return point; }
    }

    /// <summary>
    /// Gets the direction of this instance.
    /// </summary>
    /// <value>The direction.</value>
    public Direction Direction
    {
      [DebuggerStepThrough]
      get { return direction; }
    }

    /// <summary>
    /// Checks if this instance contains specified point.
    /// </summary>
    /// <param name="point">Point to check for containment.</param>
    /// <returns>
    ///   <see langword="True"/> if this instance contains the specified point; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(T point)
    {
      return this.Contains(point, comparer);
    }

    /// <summary>
    /// Checks if ray intersects with another ray (i.e. they have common part).
    /// </summary>
    /// <param name="other">Ray to check for intersection.</param>
    /// <returns>
    ///   <see langword="True"/> if ray intersects with the specified ray; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Intersects(Ray<T> other)
    {
      return this.Intersects(other, comparer);
    }

    #region IComparable<Ray<T>> members

    /// <summary>
    /// Compares this ray with <paramref name="other"/> ray.
    /// </summary>
    /// <param name="other">A <see cref="Ray{T}"/> to compare with.</param>
    /// <returns>
    /// Less than zero if <see cref="Ray{T}"/> less than<paramref name="other"/>;
    /// zero if <see cref="Ray{T}"/> equals to <paramref name="other"/>;
    /// +1 otherwise.
    /// </returns>
    public int CompareTo(Ray<T> other)
    {
      return this.CompareTo(other, comparer);
    }

    #endregion

    #region Equals, GetHashCode, ==, !=

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      return !(obj is Ray<T>) ? false : Equals((Ray<T>) obj);
    }

    /// <summary>
    /// Compares the <see cref="Ray{T}"/> with <paramref name="other"/> item.
    /// </summary>
    /// <param name="other">A <see cref="Ray{T}"/> to compare with.</param>
    /// <returns>True if the <see cref="Ray{T}"/> equals to <paramref name="other"/>;
    /// false otherwise.</returns>
    public bool Equals(Ray<T> other)
    {
      return this.EqualTo(other, comparer);
    }

    /// <summary>
    /// Implements the equality operator.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator ==(Ray<T> left, Ray<T> right)
    {
      return left.Equals(right);
    }

    /// <summary>
    /// Implements the inequality operator.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator !=(Ray<T> left, Ray<T> right)
    {
      return !left.Equals(right);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return unchecked ((point!=null ? point.GetHashCode() : 0) + 29 * direction.GetHashCode());
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return String.Format(Strings.RayFormat, point, direction);
    }



    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="point">The point of the ray.</param>
    public Ray(T point)
      : this(point, Direction.Positive)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="point">The point of the ray.</param>
    /// <param name="direction">The direction of the ray.</param>
    public Ray(T point, Direction direction)
    {
      if (direction==Direction.None)
        throw Exceptions.InvalidArgument(direction, "direction");
      this.point = point;
      this.direction = direction;
    }
  }
}
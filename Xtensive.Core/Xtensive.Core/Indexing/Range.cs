// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.05.23

using System;
using System.Diagnostics;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Resources;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Range (continuous set of points) over a set of points 
  /// of type <typeparamref name="T"/>.
  /// </summary>
  /// <typeparam name="T">A type of point.</typeparam>
  public struct Range<T> :
    IEquatable<Range<T>>,
    IComparable<Range<T>>
  {
    /// <summary>
    /// An empty <see cref="Range{T}"/> for type <typeparamref name="T"/>.
    /// </summary>
    public static readonly Range<T> Empty = default(Range<T>);
    /// <summary>
    /// Full <see cref="Range{T}"/> for type <typeparamref name="T"/>.
    /// Equals to <see cref="Empty"/>, if upper or lower range boundary for this type is absent.
    /// </summary>
    public static readonly Range<T> Full;

    private static readonly AdvancedComparer<T> comparer = AdvancedComparer<T>.System;
    private readonly Pair<T> endPoints;
    private readonly bool isNotEmpty;

    /// <summary>
    /// Indicates whether range is an empty range (contains no any point).
    /// </summary>
    public bool IsEmpty
    {
      [DebuggerStepThrough]
      get { return !isNotEmpty; }
    }

    /// <summary>
    /// Gets the endpoints of this instance.
    /// </summary>
    /// <value>The endpoints.</value>
    public Pair<T> EndPoints
    {
      [DebuggerStepThrough]
      get
      {
        if (IsEmpty)
          throw new InvalidOperationException(Strings.ExRangeIsEmpty);
        return endPoints;
      }
    }

    /// <summary>
    /// Check if range contains specified point.
    /// </summary>
    /// <param name="point">Point to check for containment.</param>
    /// <returns><see langword="True"/> if range contains specified point;
    /// otherwise, <see langword="false"/>.</returns>
    public bool Contains(T point)
    {
      return this.Contains(point, comparer);
    }

    /// <summary>
    /// Check if range intersects with the specified one.
    /// </summary>
    /// <param name="other">Range to check for intersection.</param>
    /// <returns><see langword="True"/> if range intersects with the specified one;
    /// otherwise, <see langword="false"/>.</returns>
    public bool Intersects(Range<T> other)
    {
      return this.Intersects(other, comparer);
    }

    #region IComparable<Range<T>> members

    /// <inheritdoc/>
    public int CompareTo(Range<T> other)
    {
      return this.CompareTo(other, comparer);
    }

    #endregion

    #region Equals, GetHashCode, ==, !=

    /// <summary>
    /// Compares two <see cref="Range{T}"/> instances.
    /// </summary>
    /// <param name="x">First <see cref="Range{T}"/>.</param>
    /// <param name="y">Second <see cref="Range{T}"/>.</param>
    /// <returns><see langword="true"/> if they are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(Range<T> x, Range<T> y)
    {
      return x.Equals(y);
    }

    /// <summary>
    /// Compares two <see cref="Range{T}"/> instances.
    /// </summary>
    /// <param name="x">First <see cref="Range{T}"/>.</param>
    /// <param name="y">Second <see cref="Range{T}"/>.</param>
    /// <returns><see langword="false"/> if they are equal; otherwise, <see langword="true"/>.</returns>
    public static bool operator !=(Range<T> x, Range<T> y)
    {
      return !x.Equals(y);
    }

    /// <inheritdoc/>
    public bool Equals(Range<T> other)
    {
      return this.EqualTo(other, comparer);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (!(obj is Range<T>))
        return false;
      return Equals((Range<T>)obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return endPoints.GetHashCode();
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      const string format = "({0} ... {1})";
      return String.Format(format, endPoints.First, endPoints.Second);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="endpoints">Endpoints of the range.</param>
    public Range(Pair<T> endpoints)
    {
      endPoints = endpoints;
      isNotEmpty = true;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="xPoint">First endpoint of the range.</param>
    /// <param name="yPoint">Second endpoint of the range.</param>
    public Range(T xPoint, T yPoint)
      : this(new Pair<T>(xPoint, yPoint))
    {
    }

    // Type initializer

    /// <summary>
    /// <see cref="ClassDocTemplate.TypeInitializer" copy="true" />
    /// </summary>
    static Range()
    {
      AdvancedComparer<T> comparer = AdvancedComparer<T>.Default;
      if (!(comparer.ValueRangeInfo.HasMinValue && comparer.ValueRangeInfo.HasMaxValue))
        Full = Empty;
      else
        Full = new Range<T>(comparer.ValueRangeInfo.MinValue, comparer.ValueRangeInfo.MaxValue);
    }
  }
}
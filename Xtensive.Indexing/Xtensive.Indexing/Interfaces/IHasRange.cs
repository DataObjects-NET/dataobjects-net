// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.10.17

using Xtensive.Collections;
using Xtensive.Comparison;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Should be implemented by an object that contains <see cref="Xtensive.Indexing.Range{T}"/>.
  /// </summary>
  /// <typeparam name="TObject">The type of object containing the range.</typeparam>
  /// <typeparam name="TPoint">The type of range point.</typeparam>
  public interface IHasRange<TObject,TPoint>
  {
    /// <summary>
    /// Gets range associated with the object.
    /// </summary>
    Range<TPoint> Range { get; }

    /// <summary>
    /// Merges the object with another one.
    /// </summary>
    /// <param name="other">The object to merge with.</param>
    /// <param name="comparer">Point comparer to use.</param>
    /// <returns>A sequence of up to 3 <typeparamref name="TObject"/> instances
    /// representing the result of merge.</returns>
    FixedList3<TObject> Merge(TObject other, AdvancedComparer<TPoint> comparer);

    /// <summary>
    /// Subtracts the specified object from this one.
    /// </summary>
    /// <param name="other">The object to subtract.</param>
    /// <param name="comparer">Point comparer to use.</param>
    /// <returns>A sequence of up to 3 <typeparamref name="TObject"/> instances
    /// representing the result of subtraction.</returns>
    FixedList3<TObject> Subtract(Range<TPoint> other, AdvancedComparer<TPoint> comparer);

    /// <summary>
    /// Intersects the object with another one.
    /// </summary>
    /// <param name="other">The object to intersect with.</param>
    /// <param name="comparer">Point comparer to use.</param>
    /// <returns>The result of subtraction.</returns>
    TObject Intersect(Range<TPoint> other, AdvancedComparer<TPoint> comparer);
  }
}

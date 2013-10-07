// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.05.30

using System;
using System.Diagnostics;
using Xtensive.Arithmetic;
using Xtensive.Comparison;



namespace Xtensive.Core
{
  /// <summary>
  /// A definition of segment with boundaries of type <typeparamref name="T"/>.
  /// </summary>
  /// <typeparam name="T">The type of segment boundaries.</typeparam>
  [Serializable]
  [DebuggerDisplay("Offset = {Offset}, Length = {Length}")]
  public struct Segment<T>
  {
    private static ArithmeticStruct<T> arithmetic = ArithmeticStruct<T>.Default;

    /// <summary>
    /// Segment offset.
    /// </summary>
    public readonly T Offset;
    
    /// <summary>
    /// Segment length.
    /// </summary>
    public readonly T Length;

    /// <summary>
    /// Gets <see cref="Offset"/>+<see cref="Length"/> value.
    /// </summary>
    public T EndOffset {
      [DebuggerStepThrough]
      get {
        return arithmetic.Add(Offset, Length);
      }
    }

    /// <inheritdoc/>
    public bool Equals(Pair<T> other)
    {
      return AdvancedComparerStruct<T>.System.Equals(Offset, other.First) && 
        AdvancedComparerStruct<T>.System.Equals(Length, other.Second);
    }

    /// <inheritdoc/>
    public int CompareTo(Pair<T> other)
    {
      int result = AdvancedComparerStruct<T>.System.Compare(Offset, other.First);
      if (result!=0)
        return result;
      return AdvancedComparerStruct<T>.System.Compare(Length, other.Second);
    }

    #region Equals, GetHashCode

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (obj is Pair<T>) {
        Pair<T> other = (Pair<T>)obj;
        return Equals(other);
      }
      return false;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      int firstHash = Offset == null ? 0 : Offset.GetHashCode();
      int secondHash = Length == null ? 0 : Length.GetHashCode();
      return firstHash ^ 29 * secondHash;
    }

    #endregion

    /// <summary>
    /// Implements the operator +.
    /// </summary>
    /// <param name="segment">The segment.</param>
    /// <param name="offsetShift">The offset shift.</param>
    /// <returns>The result of the operator.</returns>
    public static Segment<T> operator +(Segment<T> segment, T offsetShift)
    {
      var newOffset = arithmetic.Add(segment.Offset, offsetShift);
      return new Segment<T>(newOffset, segment.Length);
    }

    /// <summary>
    /// Implements the operator -.
    /// </summary>
    /// <param name="segment">The segment.</param>
    /// <param name="offsetShift">The offset shift.</param>
    /// <returns>The result of the operator.</returns>
    public static Segment<T> operator -(Segment<T> segment, T offsetShift)
    {
      var newOffset = arithmetic.Subtract(segment.Offset, offsetShift);
      return new Segment<T>(newOffset, segment.Length);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(Strings.SegmentFormat, Offset, Length);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="offset">Segment offset.</param>
    /// <param name="length">Segment length.</param>
    public Segment(T offset, T length)
    {
      Offset = offset;
      Length = length;
    }
  }
}
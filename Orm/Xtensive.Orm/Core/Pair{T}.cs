// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ustinov
// Created:    2007.06.01

using System;
using System.Diagnostics;
using Xtensive.Comparison;



namespace Xtensive.Core
{
  /// <summary>
  /// A pair of two values of the same type.
  /// </summary>
  /// <typeparam name="T">The type of both stored values.</typeparam>
  [Serializable]
  [DebuggerDisplay("{First}, {Second}")]
  public readonly struct Pair<T> :
    IEquatable<Pair<T>>,
    IComparable<Pair<T>>
  {
    private static readonly AdvancedComparerStruct<T> comparer = AdvancedComparerStruct<T>.System;

    /// <summary>
    /// The first value.
    /// </summary>
    public readonly T First;
    
    /// <summary>
    /// The second value.
    /// </summary>
    public readonly T Second;

    #region IComparable<...>, IEquatable<...> methods

    /// <inheritdoc/>
    public bool Equals(Pair<T> other) =>
      comparer.Equals(First, other.First) && comparer.Equals(Second, other.Second);

    /// <inheritdoc/>
    public int CompareTo(Pair<T> other)
    {
      int result = comparer.Compare(First, other.First);
      return result != 0
        ? result
        : comparer.Compare(Second, other.Second);
    }

    #endregion

    #region Equals, GetHashCode, ==, !=

    /// <inheritdoc/>
    public override bool Equals(object obj) =>
      obj is Pair<T> other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        int result = (First!=null ? First.GetHashCode() : 0);
        result = (result * 397) ^ (Second!=null ? Second.GetHashCode() : 0);
        return result;
      }
    }

    /// <summary>
    /// Checks specified objects for equality.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator ==(Pair<T> left, Pair<T> right)
    {
      return left.Equals(right);
    }

    /// <summary>
    /// Checks specified objects for inequality.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator !=(Pair<T> left, Pair<T> right)
    {
      return !left.Equals(right);
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(Strings.PairFormat, First, Second);
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this type.
    /// </summary>
    /// <param name="first">The first value in the pair.</param>
    /// <param name="second">The second value in the pair.</param>
    public Pair(T first, T second)
    {
      First = first;
      Second = second;
    }
  }
}

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
  /// A pair of two values.
  /// </summary>
  /// <typeparam name="TFirst">The <see cref="Type"/> of first value.</typeparam>
  /// <typeparam name="TSecond">The <see cref="Type"/> of second value.</typeparam>
  [Serializable]
  [DebuggerDisplay("{First}, {Second}")]
  public readonly struct Pair<TFirst, TSecond> : 
    IComparable<Pair<TFirst, TSecond>>,
    IEquatable<Pair<TFirst, TSecond>>
  {
    /// <summary>
    /// A first value.
    /// </summary>
    public readonly TFirst First;
    /// <summary>
    /// A second value.
    /// </summary>
    public readonly TSecond Second;

    #region IComparable<...>, IEquatable<...> methods

    /// <inheritdoc/>
    public bool Equals(Pair<TFirst, TSecond> other)
    {
      if (!AdvancedComparerStruct<TFirst>.System.Equals(First, other.First))
        return false;
      return AdvancedComparerStruct<TSecond>.System.Equals(Second, other.Second);
    }

    /// <inheritdoc/>
    public int CompareTo(Pair<TFirst, TSecond> other)
    {
      int result = AdvancedComparerStruct<TFirst>.System.Compare(First, other.First);
      if (result!=0)
        return result;
      return AdvancedComparerStruct<TSecond>.System.Compare(Second, other.Second);
    }

    #endregion

    #region Equals, GetHashCode, ==, !=

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (obj.GetType()!=typeof (Pair<TFirst, TSecond>))
        return false;
      return Equals((Pair<TFirst, TSecond>) obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        return ((First?.GetHashCode() ?? 0) * 397) ^ (Second?.GetHashCode() ?? 0);
      }
    }

    /// <summary>
    /// Checks specified objects for equality.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator ==(Pair<TFirst, TSecond> left, Pair<TFirst, TSecond> right)
    {
      return left.Equals(right);
    }

    /// <summary>
    /// Checks specified objects for inequality.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator !=(Pair<TFirst, TSecond> left, Pair<TFirst, TSecond> right)
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
    /// <param name="first">A first value in pair.</param>
    /// <param name="second">A second value in pair.</param>
    public Pair(TFirst first, TSecond second)
    {
      First = first;
      Second = second;
    }
  }
}

// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.06.09

using System;
using System.Diagnostics;
using Xtensive.Comparison;
using Xtensive.Internals.DocTemplates;


namespace Xtensive.Core
{
  /// <summary>
  /// Container of three values.
  /// </summary>
  /// <typeparam name="TFirst"><see cref="Type"/> of the first value.</typeparam>
  /// <typeparam name="TSecond"><see cref="Type"/> of the second value.</typeparam>
  /// <typeparam name="TThird"><see cref="Type"/> of the third value.</typeparam>
  [Serializable]
  [DebuggerDisplay("{First}, {Second}, {Third}")]
  public struct Triplet<TFirst, TSecond, TThird>: 
    IEquatable<Triplet<TFirst, TSecond, TThird>>,
    IComparable<Triplet<TFirst, TSecond, TThird>>
  {
    /// <summary>
    /// First value.
    /// </summary>
    public readonly TFirst First;
    /// <summary>
    /// Second value.
    /// </summary>
    public readonly TSecond Second;
    /// <summary>
    /// Third value.
    /// </summary>
    public readonly TThird Third;

    #region IComparable<...>, IEquatable<...> methods

    /// <inheritdoc/>
    public bool Equals(Triplet<TFirst, TSecond, TThird> other)
    {
      if (!AdvancedComparerStruct<TFirst>.System.Equals(First, other.First))
        return false;
      if (!AdvancedComparerStruct<TSecond>.System.Equals(Second, other.Second))
        return false;
      return AdvancedComparerStruct<TThird>.System.Equals(Third, other.Third);
    }

    /// <inheritdoc/>
    public int CompareTo(Triplet<TFirst, TSecond, TThird> other)
    {
      int result = AdvancedComparerStruct<TFirst>.System.Compare(First, other.First);
      if (result!=0)
        return result;
      result = AdvancedComparerStruct<TSecond>.System.Compare(Second, other.Second);
      if (result!=0)
        return result;
      return AdvancedComparerStruct<TThird>.System.Compare(Third, other.Third);
    }

    #endregion

    #region Equals, GetHashCode, ==, !=

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (obj.GetType()!=typeof (Triplet<TFirst, TSecond, TThird>))
        return false;
      return Equals((Triplet<TFirst, TSecond, TThird>) obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        int result = (First!=null ? First.GetHashCode() : 0);
        result = (result * 397) ^ (Second!=null ? Second.GetHashCode() : 0);
        result = (result * 397) ^ (Third!=null ? Third.GetHashCode() : 0);
        return result;
      }
    }

    /// <see cref="ClassDocTemplate.OperatorEq"/>
    public static bool operator ==(Triplet<TFirst, TSecond, TThird> left, Triplet<TFirst, TSecond, TThird> right)
    {
      return left.Equals(right);
    }

    /// <see cref="ClassDocTemplate.OperatorNeq"/>
    public static bool operator !=(Triplet<TFirst, TSecond, TThird> left, Triplet<TFirst, TSecond, TThird> right)
    {
      return !left.Equals(right);
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(Strings.TripletFormat, First, Second, Third);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="first">A first value in triplet.</param>
    /// <param name="second">A second value in triplet.</param>
    /// <param name="third">A third value in triplet.</param>
    public Triplet(TFirst first, TSecond second, TThird third)
    {
      First = first;
      Second = second;
      Third = third;
    }
  }
}
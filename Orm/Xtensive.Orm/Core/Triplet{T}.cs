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
  /// <typeparam name="T"><see cref="Type"/> of the triplet values.</typeparam>
  [Serializable]
  [DebuggerDisplay("{First}, {Second}, {Third}")]
  public struct Triplet<T> :
    IEquatable<Triplet<T>>,
    IComparable<Triplet<T>>
  {
    /// <summary>
    /// First value.
    /// </summary>
    public readonly T First;
    /// <summary>
    /// Second value.
    /// </summary>
    public readonly T Second;
    /// <summary>
    /// Third value.
    /// </summary>
    public readonly T Third;

    #region IComparable<...>, IEquatable<...> methods

    /// <inheritdoc/>
    public bool Equals(Triplet<T> other)
    {
      if (!AdvancedComparerStruct<T>.System.Equals(First, other.First))
        return false;
      if (!AdvancedComparerStruct<T>.System.Equals(Second, other.Second))
        return false;
      return AdvancedComparerStruct<T>.System.Equals(Third, other.Third);
    }

    /// <inheritdoc/>
    public int CompareTo(Triplet<T> other)
    {
      int result = AdvancedComparerStruct<T>.System.Compare(First, other.First);
      if (result!=0)
        return result;
      result = AdvancedComparerStruct<T>.System.Compare(Second, other.Second);
      if (result!=0)
        return result;
      return AdvancedComparerStruct<T>.System.Compare(Third, other.Third);
    }

    #endregion

    #region Equals, GetHashCode, ==, !=

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (obj.GetType()!=typeof (Triplet<T>))
        return false;
      return Equals((Triplet<T>) obj);
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
    public static bool operator ==(Triplet<T> left, Triplet<T> right)
    {
      return left.Equals(right);
    }

    /// <see cref="ClassDocTemplate.OperatorNeq"/>
    public static bool operator !=(Triplet<T> left, Triplet<T> right)
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
    public Triplet(T first, T second, T third)
    {
      First = first;
      Second = second;
      Third = third;
    }
  }
}
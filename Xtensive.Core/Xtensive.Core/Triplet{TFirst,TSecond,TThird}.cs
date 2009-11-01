// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.06.09

using System;
using Xtensive.Core.Comparison;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Resources;

namespace Xtensive.Core
{
  /// <summary>
  /// Container of three values.
  /// </summary>
  /// <typeparam name="TFirst"><see cref="Type"/> of the first value.</typeparam>
  /// <typeparam name="TSecond"><see cref="Type"/> of the second value.</typeparam>
  /// <typeparam name="TThird"><see cref="Type"/> of the third value.</typeparam>
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

    /// <inheritdoc/>
    public bool Equals(Triplet<TFirst, TSecond, TThird> other)
    {
      return
        AdvancedComparerStruct<TFirst>.System.Equals(First, other.First) &&
        AdvancedComparerStruct<TSecond>.System.Equals(Second, other.Second) &&
        AdvancedComparerStruct<TThird>.System.Equals(Third, other.Third);
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

    #region Equals, GetHashCode

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (obj is Triplet<TFirst, TSecond, TThird>) {
        var other = (Triplet<TFirst, TSecond, TThird>)obj;
        return Equals(other);
      }
      return false;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      int result = First!=null ? First.GetHashCode() : 0;
      result = 29 * result + (Second!=null ? Second.GetHashCode() : 0);
      result = 29 * result + (Third!=null ? Third.GetHashCode() : 0);
      return result;
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return String.Format(Strings.TripletFormat, First, Second, Third);
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
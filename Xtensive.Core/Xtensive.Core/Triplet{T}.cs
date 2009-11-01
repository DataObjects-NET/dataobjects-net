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
  /// <typeparam name="T"><see cref="Type"/> of the triplet values.</typeparam>
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

    /// <inheritdoc/>
    public bool Equals(Triplet<T> other)
    {
      return
        AdvancedComparerStruct<T>.System.Equals(First, other.First) &&
        AdvancedComparerStruct<T>.System.Equals(Second, other.Second) &&
        AdvancedComparerStruct<T>.System.Equals(Third, other.Third);
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

    #region Equals, GetHashCode

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (obj is Triplet<T>) {
        var other = (Triplet<T>)obj;
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
    public Triplet(T first, T second, T third)
    {
      First = first;
      Second = second;
      Third = third;
    }
  }
}
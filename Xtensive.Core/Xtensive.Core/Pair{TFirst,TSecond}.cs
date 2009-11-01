// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ustinov
// Created:    2007.06.01

using System;
using Xtensive.Core.Comparison;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Resources;

namespace Xtensive.Core
{
  /// <summary>
  /// A pair of two values.
  /// </summary>
  /// <typeparam name="TFirst">The <see cref="Type"/> of first value.</typeparam>
  /// <typeparam name="TSecond">The <see cref="Type"/> of second value.</typeparam>
  [Serializable]
  public struct Pair<TFirst, TSecond> : 
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

    /// <inheritdoc/>
    public bool Equals(Pair<TFirst, TSecond> other)
    {
      return AdvancedComparerStruct<TFirst>.System.Equals(First, other.First) && 
        AdvancedComparerStruct<TSecond>.System.Equals(Second, other.Second);
    }

    /// <inheritdoc/>
    public int CompareTo(Pair<TFirst, TSecond> other)
    {
      int result = AdvancedComparerStruct<TFirst>.System.Compare(First, other.First);
      if (result!=0)
        return result;
      return AdvancedComparerStruct<TSecond>.System.Compare(Second, other.Second);
    }

    #region Equals, GetHashCode

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (obj is Pair<TFirst, TSecond>) {
        var other = (Pair<TFirst, TSecond>)obj;
        return Equals(other);
      }
      return false;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      int firstHash = First == null ? 0 : First.GetHashCode();
      int secondHash = Second == null ? 0 : Second.GetHashCode();
      return firstHash ^ 29 * secondHash;
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return String.Format(Strings.PairFormat, First, Second);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
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
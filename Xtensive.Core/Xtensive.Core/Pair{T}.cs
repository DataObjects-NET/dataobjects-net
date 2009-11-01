// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ustinov
// Created:    2007.06.01

using System;
using System.Collections.Generic;
using Xtensive.Core.Comparison;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Resources;

namespace Xtensive.Core
{
  /// <summary>
  /// A pair of two values of the same type.
  /// </summary>
  /// <typeparam name="T">The type of both stored values.</typeparam>
  [Serializable]
  public struct Pair<T> : 
    IEquatable<Pair<T>>,
    IComparable<Pair<T>>
  {
    /// <summary>
    /// The first value.
    /// </summary>
    public readonly T First;
    
    /// <summary>
    /// The second value.
    /// </summary>
    public readonly T Second;

    /// <inheritdoc/>
    public bool Equals(Pair<T> other)
    {
      return AdvancedComparerStruct<T>.System.Equals(First, other.First) && 
        AdvancedComparerStruct<T>.System.Equals(Second, other.Second);
    }

    /// <inheritdoc/>
    public int CompareTo(Pair<T> other)
    {
      int result = AdvancedComparerStruct<T>.System.Compare(First, other.First);
      if (result!=0)
        return result;
      return AdvancedComparerStruct<T>.System.Compare(Second, other.Second);
    }

    #region Equals, GetHashCode

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (obj is Pair<T>) {
        var other = (Pair<T>)obj;
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
    /// <param name="first">The first value in the pair.</param>
    /// <param name="second">The second value in the pair.</param>
    public Pair(T first, T second)
    {
      First = first;
      Second = second;
    }
  }
}
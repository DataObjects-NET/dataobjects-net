// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.22

using System;
using System.Diagnostics;
using Xtensive.Comparison;
using Xtensive.Internals.DocTemplates;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Reversed type reverses the result provided by comparer
  /// for the original type, i.e. its values are sorted in
  /// descending order.
  /// </summary>
  /// <typeparam name="T">The type to reverse.</typeparam>
  [Serializable]
  public struct Reversed<T> : 
    IEquatable<Reversed<T>>, 
    IComparable<Reversed<T>>
  {
    private readonly T value;

    /// <summary>
    /// Gets the value of reversed type.
    /// </summary>
    public T Value {
      [DebuggerStepThrough]
      get { return value; }
    }

    /// <summary>
    /// Gets string representation of the object.
    /// </summary>
    /// <returns>String representation of the object.</returns>
    public override string ToString()
    {
      return String.Format(Strings.ReversedFormat, value);
    }

    #region Overloaded operators: Reversed<T> -> T, T -> Reversed<T>, ==, !=

    public static explicit operator T(Reversed<T> value)
    {
      return value.Value;
    }

    public static implicit operator Reversed<T>(T value)
    {
      return new Reversed<T>(value);
    }

    public static bool operator ==(Reversed<T> x, Reversed<T> y)
    {
      return x.Equals(y);
    }

    public static bool operator !=(Reversed<T> x, Reversed<T> y)
    {
      return !x.Equals(y);
    }

    #endregion

    #region IComparable<Reversed<T>> Members

    /// <summary>
    /// Compares this instance with another one.
    /// </summary>
    /// <param name="other">Instance to compare with.</param>
    /// <returns>Standard comparison result.</returns>
    public int CompareTo(Reversed<T> other)
    {
      return -AdvancedComparerStruct<T>.System.Compare(value, other.value);
    }

    #endregion

    #region Equals and GetHashCode

    public bool Equals(Reversed<T> other)
    {
      return AdvancedComparerStruct<T>.System.Equals(value, other.value);
    }

    public override bool Equals(object obj)
    {
      if (!(obj is Reversed<T>)) 
        return false;
      return Equals((Reversed<T>) obj);
    }

    public override int GetHashCode()
    {
      return value.GetHashCode();
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="value">The <see cref="Value"/> value to initialize with.</param>
    public Reversed(T value)
    {
      this.value = value;
    }
  }
}
// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.09

using System;
using System.Diagnostics;
using Xtensive.Comparison;
using Xtensive.Internals.DocTemplates;
using Xtensive.Resources;

namespace Xtensive.Core
{
  /// <summary>
  /// Packs a single <see cref="Value"/> into the <see cref="ValueType"/>.
  /// </summary>
  /// <typeparam name="T">Type of the <see cref="Value"/>.</typeparam>
  [Serializable]
  [DebuggerDisplay("{value}")]
  public struct Box<T> : 
    IEquatable<Box<T>>,
    IComparable<Box<T>>
  {
    /// <summary>
    /// Boxed value.
    /// </summary>
    public readonly T Value;

    #region IComparable<...>, IEquatable<...> methods

    /// <inheritdoc/>
    public bool Equals(Box<T> other)
    {
      return AdvancedComparerStruct<T>.System.Equals(Value, other.Value);
    }

    /// <inheritdoc/>
    public int CompareTo(Box<T> other)
    {
      return AdvancedComparerStruct<T>.System.Compare(Value, other.Value);
    }

    #endregion

    #region Equals, GetHashCode, ==, !=

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (obj.GetType()!=typeof (Box<T>))
        return false;
      return Equals((Box<T>) obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return Value!=null ? Value.GetHashCode() : 0;
    }

    /// <see cref="ClassDocTemplate.OperatorEq"/>
    public static bool operator ==(Box<T> left, Box<T> right)
    {
      return left.Equals(right);
    }

    /// <see cref="ClassDocTemplate.OperatorNeq"/>
    public static bool operator !=(Box<T> left, Box<T> right)
    {
      return !left.Equals(right);
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(Strings.BoxFormat, Value);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="value">Boxed value.</param>
    public Box(T value)
    {
      Value = value;
    }
  }
}
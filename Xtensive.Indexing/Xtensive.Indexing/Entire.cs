// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.22

using System;
using System.Diagnostics;
using System.Text;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Provides additional positive and negative infinity 
  /// values or infinitesimal shift values to its type parameter <typeparamref name="T"/>.
  /// Much like <see cref="Nullable{T}"/>, but for describing
  /// <see cref="Entire{T}"/> value type.
  /// </summary>
  /// <typeparam name="T">The type to extend with <see cref="ValueType"/> information.</typeparam>
  [Serializable]
  public struct Entire<T> : 
    IComparable<T>,
    IEquatable<T>,
    IComparable<Entire<T>>,
    IEquatable<Entire<T>>,
    ICloneable
  {
    private static readonly object typeLock = new object();
    private static readonly Entire<T> minValue = new Entire<T>(InfinityType.Negative);
    private static readonly Entire<T> maxValue = new Entire<T>(InfinityType.Positive);
    private static Func<Entire<T>, T, int> asymmetricCompare;
    private readonly bool hasValue;
    internal readonly T value;
    internal readonly EntireValueType valueType;

    #region MinValue \ MaxValue

    /// <summary>
    /// The smallest possible value of <see cref="Entire{T}"/> (negative infinity). 
    /// </summary>
    /// <value>The negative infinity.</value>
    public static Entire<T> MinValue {
      [DebuggerStepThrough]
      get {
        return minValue;
      }
    }

    /// <summary>
    /// The largest possible value of <see cref="Entire{T}"/> (positive infinity). 
    /// </summary>
    /// <value>The positive infinity.</value>
    public static Entire<T> MaxValue {
      [DebuggerStepThrough]
      get { 
        return maxValue;
      }
    }

    #endregion
    
    /// <inheritdoc/>
    public T Value {
      [DebuggerStepThrough]
      get {
        return value;
      }
    }

    /// <inheritdoc/>
    public EntireValueType ValueType
    {
      [DebuggerStepThrough]
      get { return valueType; }
    }

    /// <inheritdoc/>
    public bool HasValue
    {
      [DebuggerStepThrough]
      get { return hasValue; }
    }

    #region Clone

    /// <inheritdoc/>
    public Entire<T> Clone()
    {
      return new Entire<T>(value, valueType);
    }

    /// <inheritdoc/>
    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion

    #region Operators, conversion

    /// <summary>
    /// Performs an explicit conversion from <see cref="Entire{T}"/> to <typeparamref name="T"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator T(Entire<T> value)
    {
      return value.Value;
    }

    /// <summary>
    /// Performs an implicit conversion from <typeparamref name="T"/> to <see cref="Entire{T}"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static implicit operator Entire<T>(T value)
    {
      return new Entire<T>(value);
    }

    /// <summary>
    /// Implements the equality operator.
    /// </summary>
    /// <param name="x">The first argument.</param>
    /// <param name="y">The second argument.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator ==(Entire<T> x, Entire<T> y)
    {
      return x.Equals(y);
    }

    /// <summary>
    /// Implements the inequality operator.
    /// </summary>
    /// <param name="x">The first argument.</param>
    /// <param name="y">The second argument.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator !=(Entire<T> x, Entire<T> y)
    {
      return !x.Equals(y);
    }

    #endregion

    #region IComparable, IEquatable

    /// <inheritdoc/>
    public int CompareTo(Entire<T> other)
    {
      return AdvancedComparerStruct<Entire<T>>.System.Compare(this, other);
    }

    /// <inheritdoc/>
    int IComparable<Entire<T>>.CompareTo(Entire<T> other)
    {
      return AdvancedComparerStruct<Entire<T>>.System.Compare(this, other);
    }

    /// <inheritdoc/>
    public int CompareTo(T other)
    {
      return AsymmetricCompare(this, other);
    }

    /// <inheritdoc/>
    public bool Equals(Entire<T> other)
    {
      return AdvancedComparerStruct<Entire<T>>.System.Equals(this, other);
    }

    /// <inheritdoc/>
    bool IEquatable<Entire<T>>.Equals(Entire<T> other)
    {
      return AdvancedComparerStruct<Entire<T>>.System.Equals(this, other);
    }

    /// <inheritdoc/>
    public bool Equals(T other)
    {
      return AsymmetricCompare(this, other)==0;
    }

    #endregion

    #region Equals, GetHashCode

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (obj==null)
        return Equals((T)obj); // Assymetric comparison for nullable & class types
      if (obj is Entire<T>)
        return Equals((Entire<T>)obj);
      if (obj is T)
        return Equals((T)obj); // Assymetric comparison
      return false;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return (value==null ? 0 : value.GetHashCode()) ^ (int) valueType;
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return ToString(this);
    }

    #region Private \ internal methods

    internal static Func<Entire<T>, T, int> AsymmetricCompare {
      get {
        if (asymmetricCompare==null) lock (typeLock) if (asymmetricCompare==null) {
          asymmetricCompare = AdvancedComparer<Entire<T>>.System.GetAsymmetric<T>();
        }
        return asymmetricCompare;
      }
    }

    internal static string ToString(Entire<T> entire)
    {
      var sb = new StringBuilder(16);
      sb.AppendFormat("{0}", entire.Value);
      if (entire.ValueType.IsInfinity()) {
        sb.Append((int) entire.ValueType < 0 ? "-" : "+");
        sb.Append(Strings.Infinity);
      }
      else if (entire.ValueType != EntireValueType.Exact)
        sb.Append((int)entire.ValueType < 0 ? "-" : "+");
      return String.Format(Strings.EntireFormat, sb);
    }

    #endregion

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="infinityType">Type of the infinity.</param>
    public Entire(InfinityType infinityType)
    {
      if (infinityType == InfinityType.None)
        throw new ArgumentException(Strings.ExCantPassNoInfinityToThisConstructor, "infinityType");
      valueType = infinityType == InfinityType.Positive
        ? EntireValueType.PositiveInfinity
        : EntireValueType.NegativeInfinity;
      value = default(T);
      hasValue = false;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="value">The value.</param>
    public Entire(T value)
    {
      valueType = EntireValueType.Exact;
      this.value = value;
      hasValue = true;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="lastValueType"><see cref="ValueType"/> property value.</param>
    public Entire(T value, EntireValueType lastValueType)
    {
      valueType = lastValueType;
      if (lastValueType.IsInfinity()) {
        this.value = default(T);
        hasValue = false;
      }
      else {
        this.value = value;
        hasValue = true;
      }
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="infinitesimalShiftDirection">Infinitesimal shift direction. Can't have value Direction.None.</param>
    public Entire(T value, Direction infinitesimalShiftDirection)
    {
      if (infinitesimalShiftDirection==Direction.None)
        throw Exceptions.InvalidArgument(infinitesimalShiftDirection, "infinitesimalShiftDirection");

      valueType = infinitesimalShiftDirection == Direction.Positive
        ? EntireValueType.PositiveInfinitesimal
        : EntireValueType.NegativeInfinitesimal;
      this.value = value;
      hasValue = true;
    }
  }
}
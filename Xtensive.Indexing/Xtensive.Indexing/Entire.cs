// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.22

using System;
using System.Diagnostics;
using System.Text;
using Xtensive.Core;
using Xtensive.Core.Comparison;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Provides additional positive and negative infinity 
  /// values or infinitesimal shift values to its type parameter <see cref="T"/>.
  /// Much like <see cref="Nullable{T}"/>, but for describing
  /// <see cref="Entire{T}"/> value type.
  /// </summary>
  /// <typeparam name="T">The type to extend with <see cref="ValueType"/> information.</typeparam>
  [Serializable]
  public struct Entire<T> : 
    IEntire<T>
  {
    private static readonly object typeLock = new object();
    private static readonly TupleDescriptor defaultDescriptor = TupleDescriptor.Create(new Type[] {typeof (T)});
    private static readonly IEntire<T> minValue = Create(InfinityType.Negative);
    private static readonly IEntire<T> maxValue = Create(InfinityType.Positive);
    private static Func<Entire<T>, T, int> asymmetricCompare;
    internal readonly T value;
    internal readonly EntireValueType valueType;
    internal TupleDescriptor descriptor;
    private static IEntireFactory<T> factory;

    #region MinValue \ MaxValue

    /// <summary>
    /// The smallest possible value of <see cref="Entire{T}"/> (negative infinity). 
    /// </summary>
    /// <value>The negative infinity.</value>
    public static IEntire<T> MinValue {
      [DebuggerStepThrough]
      get {
        return minValue;
      }
    }

    /// <summary>
    /// The largest possible value of <see cref="Entire{T}"/> (positive infinity). 
    /// </summary>
    /// <value>The positive infinity.</value>
    public static IEntire<T> MaxValue {
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
    public EntireValueType[] ValueTypes {
      [DebuggerStepThrough]
      get {
        return new EntireValueType[] {valueType};
      }
    }

    #region IEntire.GetXxx methods

    /// <inheritdoc/>
    EntireValueType IEntire<T>.GetValueType(int fieldIndex)
    {
      return valueType;
    }

    #endregion

    #region ITuple.Descriptor, Count, indexer

    /// <inheritdoc/>
    TupleDescriptor ITuple.Descriptor
    {
      [DebuggerStepThrough]
      get { return descriptor; }
    }

    /// <inheritdoc/>
    int ITuple.Count
    {
      [DebuggerStepThrough]
      get { return 1; }
    }

    /// <inheritdoc/>
    object ITuple.this[int fieldIndex]
    {
      get { return ((ITuple)this).GetValue(fieldIndex); }
      set { ((ITuple)this).SetValue(fieldIndex, value); }
    }

    #endregion

    #region ITuple.IsXxx \ HasXxx methods

    /// <inheritdoc/>
    bool ITuple.IsAvailable(int fieldIndex)
    {
      return HasValue;
    }

    /// <inheritdoc/>
    bool ITuple.IsNull(int fieldIndex)
    {
      EnsureHasValue();
      return Value==null;
    }

    /// <inheritdoc/>
    bool ITuple.HasValue(int fieldIndex)
    {
      return HasValue;
    }

    #endregion

    #region ITuple.GetXxx methods

    public TupleFieldState GetFieldState(int fieldIndex)
    {
      if (!HasValue)
        return 0;
      if (value==null)
        return TupleFieldState.IsAvailable | TupleFieldState.IsNull;
      return TupleFieldState.IsAvailable;
    }

    /// <inheritdoc/>
    object ITuple.GetValue(int fieldIndex)
    {
      EnsureHasValue();
      return Value;
    }

    /// <inheritdoc/>
    object ITuple.GetValueOrDefault(int fieldIndex)
    {
      return Value;
    }

    #endregion

    #region ITuple.SetXxx methods

    /// <inheritdoc/>
    void ITuple.SetValue(int fieldIndex, object fieldValue)
    {
      throw Exceptions.ObjectIsReadOnly(null);
    }

    #endregion

    #region Clone \ ITuple.CreateNew methods

    /// <inheritdoc/>
    public Entire<T> Clone()
    {
      return new Entire<T>(value, valueType);
    }

    /// <inheritdoc/>
    ITuple ITuple.Clone()
    {
      return Clone();
    }

    /// <inheritdoc/>
    object ICloneable.Clone()
    {
      return Clone();
    }

    /// <inheritdoc/>
    ITuple ITupleFactory.CreateNew()
    {
      throw Exceptions.ObjectIsReadOnly(null);
    }

    #endregion

    #region Operators, conversion

    /// <summary>
    /// Performs an explicit conversion from <see cref="Entire{T}"/> to <see cref="T"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator T(Entire<T> value)
    {
      return value.Value;
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="T"/> to <see cref="Entire{T}"/>.
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
    int IComparable<IEntire<T>>.CompareTo(IEntire<T> other)
    {
      if (other == null)
        return 1;
      if (other is Entire<T>)
        return AdvancedComparerStruct<Entire<T>>.System.Compare(this, (Entire<T>)other);
      else 
        return AdvancedComparerStruct<IEntire<T>>.System.Compare(this, other);
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
    bool IEquatable<IEntire<T>>.Equals(IEntire<T> other)
    {
      if (other == null)
        return false;
      if (other is Entire<T>)
        return AdvancedComparerStruct<Entire<T>>.System.Equals(this, (Entire<T>)other);
      else 
        return AdvancedComparerStruct<IEntire<T>>.System.Equals(this, other);
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
      else if (obj is Entire<T>)
        return Equals((Entire<T>)obj);
      else if (obj is T)
        return Equals((T)obj); // Assymetric comparison
      else if (obj is IEntire<T>)
        return AdvancedComparerStruct<IEntire<T>>.System.Equals(this, (IEntire<T>)obj);
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

    internal bool HasValue {
      [DebuggerStepThrough]
      get {
        return 
          valueType!=EntireValueType.PositiveInfinity && 
          valueType!=EntireValueType.NegativeInfinity;
      }
    }

    internal void EnsureHasValue()
    {
      if (!HasValue)
        throw new InvalidOperationException(Strings.ExValueIsNotAvailable);
    }

    internal static Func<Entire<T>, T, int> AsymmetricCompare {
      get {
        if (asymmetricCompare==null) lock (typeLock) if (asymmetricCompare==null) {
          asymmetricCompare = AdvancedComparer<Entire<T>>.System.GetAsymmetric<T>();
        }
        return asymmetricCompare;
      }
    }

    internal static string ToString(IEntire<T> entire)
    {
      StringBuilder sb = new StringBuilder(16);
      for (int i = 0; i<entire.Count; i++) {
        if (i>0)
          sb.Append(", ");
        EntireValueType valueType = entire.GetValueType(i);
        switch (valueType) {
        case EntireValueType.NegativeInfinity:
        case EntireValueType.PositiveInfinity:
          sb.Append((int)valueType < 0 ? "-" : "+");
          sb.Append(Strings.Infinity);
          break;
        case EntireValueType.NegativeInfinitesimal:
        case EntireValueType.PositiveInfinitesimal:
        case EntireValueType.Exact:
          string value;
          if (!entire.IsAvailable(i))
            value = Strings.NotAvailable;
          else if (entire.IsNull(i))
            value = Strings.Null;
          else
            value = entire.GetValue(i).ToString();
          if (valueType==EntireValueType.Exact)
            sb.Append(value);
          else
            sb.Append(String.Format(Strings.InfinitesimalFormat, value, (int)valueType < 0 ? "-" : "+"));
          break;
        default:
          throw Exceptions.InternalError("Unknown EntireValueType.", Log.Instance);
        }
      }
      return String.Format(Strings.EntireFormat, sb);
    }

    #endregion

    #region Create methods (factory provider invokers)

    private static IEntireFactory<T> Factory {
      get {
        if (factory == null) lock (typeLock) if (factory == null)
          factory = EntireFactoryProvider.Default.GetFactory<T>();  
        return factory;
      }
    }

    public static IEntire<T> Create(T value)
    {
      return Factory.CreateEntire(value);
    }

    public static IEntire<T> Create(InfinityType infinityType)
    {
      return Factory.CreateEntire(infinityType);
    }

    public static IEntire<T> Create(T value, Direction infinitesimalShiftDirection)
    {
      return Factory.CreateEntire(value, infinitesimalShiftDirection);
    }

    public static IEntire<T> Create(T value, params EntireValueType[] fieldValueTypes)
    {
      return Factory.CreateEntire(value, fieldValueTypes);
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
      descriptor = defaultDescriptor;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="value">The value.</param>
    public Entire(T value)
    {
      valueType = EntireValueType.Exact;
      this.value = value;
      descriptor = defaultDescriptor;
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
      descriptor = defaultDescriptor;
    }

    internal Entire(T value, EntireValueType valueType)
    {
      this.valueType = valueType;
      this.value = value;
      descriptor = defaultDescriptor;
    }
  }
}
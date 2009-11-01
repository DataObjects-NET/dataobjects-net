// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.28

using System;
using Xtensive.Core;
using Xtensive.Core.Comparison;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Indexing.Resources;
using Xtensive.Core.Collections;

namespace Xtensive.Indexing
{
  [Serializable]
  internal sealed class TupleEntire : IEntire<Tuple>,
    IComparable<TupleEntire>,
    IEquatable<TupleEntire>
  {
    private static readonly object typeLock = new object();
    private static readonly Tuple defaultTuple = Tuple.Create(0);
    private static readonly EntireValueType[] positiveInfinityFieldValueTypes = new EntireValueType[] { EntireValueType.PositiveInfinity };
    private static readonly EntireValueType[] negativeInfinityFieldValueTypes = new EntireValueType[] { EntireValueType.NegativeInfinity };
    private static Func<IEntire<Tuple>, Tuple, int> asymmetricCompare;
    private readonly Tuple tuple;
    private readonly EntireValueType[] valueTypes;
    private int cachedHashCode;

    /// <inheritdoc/>
    public TupleDescriptor Descriptor
    {
      get { return tuple.Descriptor; }
    }

    /// <inheritdoc/>
    public Tuple Value
    {
      get { return tuple.Clone(); }
    }

    /// <inheritdoc/>
    public EntireValueType[] ValueTypes
    {
      get { return valueTypes.Copy(); }
    }

    /// <inheritdoc/>
    public int Count
    {
      get { return tuple.Count; }
    }

    /// <inheritdoc/>
    public object this[int fieldIndex]
    {
      get { return GetValue(fieldIndex); }
      set { SetValue(fieldIndex, value); }
    }

    #region IsXxx \ HasXxx methods

    /// <inheritdoc/>
    public bool IsAvailable(int fieldIndex)
    {
      if (IsInfinite(fieldIndex))
        return false;
      return tuple.IsAvailable(fieldIndex);
    }

    /// <inheritdoc/>
    public bool IsNull(int fieldIndex)
    {
      EnsureNotInfinite(fieldIndex);
      return tuple.IsNull(fieldIndex);
    }

    /// <inheritdoc/>
    public bool HasValue(int fieldIndex)
    {
      if (IsInfinite(fieldIndex))
        return false;
      return tuple.HasValue(fieldIndex);
    }

    #endregion

    #region GetXxx methods

    /// <inheritdoc/>
    public EntireValueType GetValueType(int fieldIndex)
    {
      return valueTypes[fieldIndex];
    }

    /// <inheritdoc/>
    public TFieldType GetValue<TFieldType>(int fieldIndex)
    {
      EnsureNotInfinite(fieldIndex);
      return tuple.GetValue<TFieldType>(fieldIndex);
    }

    /// <inheritdoc/>
    public object GetValue(int fieldIndex)
    {
      EnsureNotInfinite(fieldIndex);
      return tuple.GetValue(fieldIndex);
    }

    /// <inheritdoc/>
    public TFieldType GetValueOrDefault<TFieldType>(int fieldIndex)
    {
      return tuple.GetValueOrDefault<TFieldType>(fieldIndex);
    }

    /// <inheritdoc/>
    public object GetValueOrDefault(int fieldIndex)
    {
      return tuple.GetValueOrDefault(fieldIndex);
    }

    public TupleFieldState GetFieldState(int fieldIndex)
    {
      return tuple.GetFieldState(fieldIndex);
    }

    /// <inheritdoc/>
    public Entire<TFieldType> GetEntire<TFieldType>(int fieldIndex)
    {
      return new Entire<TFieldType>(GetValue<TFieldType>(fieldIndex), GetValueType(fieldIndex));
    }

    #endregion

    #region SetXxx methods

    /// <inheritdoc/>
    public void SetValue<TFieldType>(int fieldIndex, TFieldType fieldValue)
    {
      throw Exceptions.ObjectIsReadOnly(null);
    }

    /// <inheritdoc/>
    public void SetValue(int fieldIndex, object fieldValue)
    {
      throw Exceptions.ObjectIsReadOnly(null);
    }

    #endregion

    #region CreateNew, Clone

    /// <inheritdoc/>
    public ITuple CreateNew()
    {
      throw Exceptions.ObjectIsReadOnly(null);
    }

    /// <inheritdoc/>
    public ITuple Clone()
    {
      TupleEntire clone = new TupleEntire(tuple, valueTypes);
      return clone;
    }

    /// <inheritdoc/>
    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion

    #region IComparable, IEquatable

    /// <inheritdoc/>
    public int CompareTo(TupleEntire other)
    {
      return AdvancedComparerStruct<IEntire<Tuple>>.System.Compare(this, other);
    }

    /// <inheritdoc/>
    public int CompareTo(IEntire<Tuple> other)
    {
      return AdvancedComparerStruct<IEntire<Tuple>>.System.Compare(this, other);
    }

    /// <inheritdoc/>
    public int CompareTo(Tuple other)
    {
      return AsymmetricCompare(this, other);
    }

    /// <inheritdoc/>
    public bool Equals(TupleEntire other)
    {
      return AdvancedComparerStruct<IEntire<Tuple>>.System.Equals(this, other);
    }

    /// <inheritdoc/>
    public bool Equals(IEntire<Tuple> other)
    {
      return AdvancedComparerStruct<IEntire<Tuple>>.System.Equals(this, other);
    }

    /// <inheritdoc/>
    public bool Equals(Tuple other)
    {
      return AsymmetricCompare(this, other)==0;
    }

    #endregion

    #region Equals, GetHashCode

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      return Equals(obj as TupleEntire);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      if (cachedHashCode==0)
        UpdateCachedHashCode();
      return cachedHashCode;
    }

    #endregion

    #region Private \ internal methods 

    internal static Func<IEntire<Tuple>, Tuple, int> AsymmetricCompare {
      get {
        if (asymmetricCompare==null) lock (typeLock) if (asymmetricCompare==null) {
          asymmetricCompare = AdvancedComparer<IEntire<Tuple>>.System.GetAsymmetric<Tuple>();
        }
        return asymmetricCompare;
      }
    }

    private void EnsureNotInfinite(int fieldIndex)
    {
      if (IsInfinite(fieldIndex))
        throw new InvalidOperationException(String.Format(Strings.ExFieldIsInfinite, fieldIndex));
    }

    private bool IsInfinite(int fieldIndex)
    {
      EntireValueType fieldValueType = GetValueType(fieldIndex);
      if (fieldValueType == EntireValueType.PositiveInfinity || fieldValueType == EntireValueType.NegativeInfinity)
        return true;
      return false;
    }

    private void UpdateCachedHashCode()
    {
      cachedHashCode = 0;
      if (tuple!=null) {
        cachedHashCode ^= tuple.GetHashCode();
        int count = Count;
        for (int i = 0; i<count; i++)
          cachedHashCode ^= (int)valueTypes[i] << i;
      }
      if (cachedHashCode==0)
        cachedHashCode = -1;
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return Entire<Tuple>.ToString(this);
    }


    // Constructors

    public TupleEntire(Tuple value)
      : this(value, null)
    {
    }

    public TupleEntire(InfinityType infinityType)
      : this(null, null)
    {
      if (infinityType == InfinityType.None)
        throw new ArgumentException(Strings.ExCantPassNoInfinityToThisConstructor, "infinityType");
      
      if (infinityType == InfinityType.Negative)
        valueTypes = negativeInfinityFieldValueTypes;
      else
        valueTypes = positiveInfinityFieldValueTypes;
    }

    public TupleEntire(Tuple value, int infinitesimallyShiftedFieldIndex, Direction infinitesimalShiftDirection)
      : this(value, null)
    {
      ArgumentValidator.EnsureArgumentIsInRange(infinitesimallyShiftedFieldIndex, 0, Count-1, "infinitesimallyShiftedFieldIndex");
      if (infinitesimalShiftDirection == Direction.None)
        throw Exceptions.InvalidArgument(infinitesimalShiftDirection, "infinitesimalShiftDirection");

      EntireValueType shiftedFieldValueType;
      if (infinitesimalShiftDirection == Direction.Positive)
        shiftedFieldValueType = EntireValueType.PositiveInfinitesimal;
      else
        shiftedFieldValueType = EntireValueType.NegativeInfinitesimal;
      valueTypes[infinitesimallyShiftedFieldIndex] = shiftedFieldValueType;
    }

    public TupleEntire(Tuple value, Direction infinitesimalShiftDirection)
      : this(value, value.Count-1, infinitesimalShiftDirection)
    {
    }

    public TupleEntire(Tuple value, params EntireValueType[] fieldValueTypes)
    {
      int count;
      if (value==null) {
        tuple = defaultTuple;
        count = 1;
      }
      else if (value.Count<1) {
        tuple = defaultTuple;
        count = 1;
      }
      else {
        tuple = value;
        count = value.Count;
      }
      if (fieldValueTypes==null)
        this.valueTypes = new EntireValueType[count];
      else if (fieldValueTypes.Length==count) 
        this.valueTypes = fieldValueTypes;
      else {
        this.valueTypes = new EntireValueType[count];
        fieldValueTypes.CopyTo(this.valueTypes, 0);
      }
    }
  }
}
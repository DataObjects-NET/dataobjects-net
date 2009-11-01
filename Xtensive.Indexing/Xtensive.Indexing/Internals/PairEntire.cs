// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.28

using System;
using Xtensive.Core;
using Xtensive.Core.Comparison;
using Xtensive.Core.Tuples;
using Xtensive.Indexing.Resources;
using Xtensive.Core.Collections;

namespace Xtensive.Indexing
{
  [Serializable]
  internal struct PairEntire<TFirst, TSecond> : IEntire<Pair<TFirst, TSecond>>,
    IComparable<PairEntire<TFirst, TSecond>>,
    IEquatable<PairEntire<TFirst, TSecond>>
  {
    private static readonly object typeLock = new object();
    private static readonly EntireValueType[] positiveInfinityFieldValueTypes = new EntireValueType[] { EntireValueType.PositiveInfinity };
    private static readonly EntireValueType[] negativeInfinityFieldValueTypes = new EntireValueType[] { EntireValueType.NegativeInfinity };
    private static Func<IEntire<Pair<TFirst, TSecond>>, Pair<TFirst, TSecond>, int> asymmetricCompare;
    private readonly TupleDescriptor descriptor;
    private readonly Pair<TFirst, TSecond> pair;
    private readonly int count;
    private readonly EntireValueType[] fieldValueTypes;

    /// <inheritdoc/>
    public TupleDescriptor Descriptor
    {
      get { return descriptor; }
    }

    /// <inheritdoc/>
    public Pair<TFirst, TSecond> Value
    {
      get { return pair; }
    }

    /// <inheritdoc/>
    public EntireValueType[] ValueTypes
    {
      get { return fieldValueTypes.Copy(); }
    }

    /// <inheritdoc/>
    public int Count
    {
      get { return count; }
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
      return HasValue(fieldIndex);
    }

    /// <inheritdoc/>
    public bool IsNull(int fieldIndex)
    {
      return HasValue(fieldIndex) && GetValue(fieldIndex) != null;
    }

    /// <inheritdoc/>
    public bool HasValue(int fieldIndex)
    {
      ArgumentValidator.EnsureArgumentIsInRange(fieldIndex, 0, Count, "fieldIndex");
      return !IsInfinite(fieldIndex);
    }

    #endregion

    #region GetXxx methods

    /// <inheritdoc/>
    public TupleFieldState GetFieldState(int fieldIndex)
    {
      return IsNull(fieldIndex)? TupleFieldState.IsNull : TupleFieldState.IsAvailable;
    }

    /// <inheritdoc/>
    public EntireValueType GetValueType(int fieldIndex)
    {
      ArgumentValidator.EnsureArgumentIsInRange(fieldIndex, 0, Count, "fieldIndex");
      return fieldValueTypes[fieldIndex];
    }

    /// <inheritdoc/>
    public TFieldType GetValue<TFieldType>(int fieldIndex)
    {
      return (TFieldType)GetValue(fieldIndex);
    }

    /// <inheritdoc/>
    public object GetValue(int fieldIndex)
    {
      ArgumentValidator.EnsureArgumentIsInRange(fieldIndex, 0, 1, "fieldIndex");
      EnsureNotInfinite(fieldIndex);
      if (fieldIndex == 0)
        return pair.First;
      return pair.Second;
    }

    /// <inheritdoc/>
    public TFieldType GetValueOrDefault<TFieldType>(int fieldIndex)
    {
      return (TFieldType)GetValue(fieldIndex);
    }

    /// <inheritdoc/>
    public object GetValueOrDefault(int fieldIndex)
    {
      ArgumentValidator.EnsureArgumentIsInRange(fieldIndex, 0, 1, "fieldIndex");
      EnsureNotInfinite(fieldIndex);
      if (fieldIndex == 0)
        return pair.First;
      return pair.Second;
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
      PairEntire<TFirst, TSecond> clone = new PairEntire<TFirst, TSecond>(this);
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
    public int CompareTo(PairEntire<TFirst, TSecond> other)
    {
      return AdvancedComparerStruct<IEntire<Pair<TFirst, TSecond>>>.System.Compare(this, other);
    }

    /// <inheritdoc/>
    public int CompareTo(IEntire<Pair<TFirst, TSecond>> other)
    {
      return AdvancedComparerStruct<IEntire<Pair<TFirst, TSecond>>>.System.Compare(this, other);
    }

    /// <inheritdoc/>
    public int CompareTo(Pair<TFirst, TSecond> other)
    {
      return AsymmetricCompare(this, other);
    }

    /// <inheritdoc/>
    public bool Equals(PairEntire<TFirst, TSecond> other)
    {
      return AdvancedComparerStruct<IEntire<Pair<TFirst, TSecond>>>.System.Equals(this, other);
    }

    /// <inheritdoc/>
    public bool Equals(IEntire<Pair<TFirst, TSecond>> other)
    {
      return AdvancedComparerStruct<IEntire<Pair<TFirst, TSecond>>>.System.Equals(this, other);
    }

    /// <inheritdoc/>
    public bool Equals(Pair<TFirst, TSecond> other)
    {
      return AsymmetricCompare(this, other)==0;
    }

    #endregion

    #region Equals, GetHashCode

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (obj==null)
        return false;
      else if (obj is Pair<TFirst, TSecond>)
        return Equals((PairEntire<TFirst, TSecond>)obj);
      else if (obj is PairEntire<TFirst, TSecond>)
        return Equals((Pair<TFirst, TSecond>)obj); // Assymetric comparison
      else if (obj is IEntire<Pair<TFirst, TSecond>>)
        return AdvancedComparerStruct<IEntire<Pair<TFirst, TSecond>>>.System.Equals(this, (IEntire<Pair<TFirst, TSecond>>)obj);
      return false;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return ((pair.First==null ? 0 : pair.First.GetHashCode()) ^ (int) fieldValueTypes[0]) ^ 
        ((pair.Second==null ? 0 : pair.Second.GetHashCode()) ^ (int) fieldValueTypes[1]);
    }

    #endregion

    #region Private \ internal methods 

    internal static Func<IEntire<Pair<TFirst, TSecond>>, Pair<TFirst, TSecond>, int> AsymmetricCompare {
      get {
        if (asymmetricCompare==null) lock (typeLock) if (asymmetricCompare==null) {
          asymmetricCompare = AdvancedComparer<IEntire<Pair<TFirst, TSecond>>>.System.GetAsymmetric<Pair<TFirst, TSecond>>();
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

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return Entire<Pair<TFirst, TSecond>>.ToString(this);
    }


    // Constructors

    public PairEntire(Pair<TFirst, TSecond> value)
      : this(value, true)
    {
    }

    public PairEntire(InfinityType infinityType)
      : this(default(Pair<TFirst, TSecond>), true)
    {
      if (infinityType == InfinityType.None)
        throw new ArgumentException(Strings.ExCantPassNoInfinityToThisConstructor, "infinityType");

      if (infinityType == InfinityType.Negative)
        fieldValueTypes = negativeInfinityFieldValueTypes;
      else
        fieldValueTypes = positiveInfinityFieldValueTypes;
    }

    public PairEntire(Pair<TFirst, TSecond> value, int infinitesimallyShiftedFieldIndex, Direction infinitesimalShiftDirection)
      : this(value, true)
    {
      ArgumentValidator.EnsureArgumentIsInRange(infinitesimallyShiftedFieldIndex, 0, count-1, "infinitesimallyShiftedFieldIndex");
      if (infinitesimalShiftDirection == Direction.None)
        throw Exceptions.InvalidArgument(infinitesimalShiftDirection, "infinitesimalShiftDirection");

      if (count==0)
        return;
      EntireValueType shiftedFieldValueType;
      if (infinitesimalShiftDirection == Direction.Positive)
        shiftedFieldValueType = EntireValueType.PositiveInfinitesimal;
      else
        shiftedFieldValueType = EntireValueType.NegativeInfinitesimal;
      fieldValueTypes[infinitesimallyShiftedFieldIndex] = shiftedFieldValueType;
    }

    public PairEntire(Pair<TFirst, TSecond> value, Direction infinitesimalShiftDirection)
      : this(value, 1, infinitesimalShiftDirection)
    {
    }

    public PairEntire(Pair<TFirst, TSecond> value, params EntireValueType[] fieldValueTypes)
      : this(value, true)
    {
      ArgumentValidator.EnsureArgumentIsInRange(fieldValueTypes.Length, 0, this.fieldValueTypes.Length, "fieldValueTypes.Length");
      fieldValueTypes.CopyTo(this.fieldValueTypes, 0);
    }

    public PairEntire(PairEntire<TFirst, TSecond> source)
      : this (source.pair)
    {
      fieldValueTypes = source.fieldValueTypes;
    }

    private PairEntire(Pair<TFirst, TSecond> value, bool ignoreMe)
    {
      pair = value;
      count = 2;
      descriptor = TupleDescriptor.Create(new Type[] { typeof(TFirst), typeof(TSecond) });
      fieldValueTypes = new EntireValueType[count];
    }
  }
}
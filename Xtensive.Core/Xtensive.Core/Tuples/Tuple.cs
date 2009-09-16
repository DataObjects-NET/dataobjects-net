// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.05.30

using System;
using System.Diagnostics;
using System.Text;
using Xtensive.Core.Comparison;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Resources;

namespace Xtensive.Core.Tuples
{
  /// <summary>
  /// A base class for auto generated tuples.
  /// </summary>
  public abstract class Tuple : ITuple,
    IEquatable<Tuple>,
    IComparable<Tuple>
  {
    /// <summary>
    /// Per-field hash code multiplier used in <see cref="GetHashCode"/> calculation.
    /// </summary>
    public const int HashCodeMultiplier = 397;

    /// <inheritdoc />
    public abstract TupleDescriptor Descriptor { get; }

    /// <inheritdoc />
    public virtual int Count { 
      [DebuggerStepThrough]
      get { return Descriptor.Count; }
    }

    /// <inheritdoc/>
    public object this[int fieldIndex]
    {
      [DebuggerStepThrough]
      get { return GetValue(fieldIndex); }
      [DebuggerStepThrough]
      set { SetValue(fieldIndex, value); }
    }

    #region CreateNew, Clone methods

    /// <inheritdoc/>
    ITuple ITupleFactory.CreateNew()
    {
      return CreateNew();
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

    /// <see cref="ITupleFactory.CreateNew" copy="true" />
    public virtual Tuple CreateNew()
    {
      return Create(Descriptor);
    }

    /// <see cref="ITuple.Clone" copy="true" />
    public virtual Tuple Clone()
    {
      return (Tuple) MemberwiseClone();
    }

    #endregion

    #region IsXxx \ HasXxx methods

    /// <inheritdoc/>
    public bool IsAvailable(int fieldIndex)
    {
      return (GetFieldState(fieldIndex) & TupleFieldState.Available) != 0;
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Field value is not available.</exception>
    public bool IsNull(int fieldIndex)
    {
      var state = GetFieldState(fieldIndex);
      if ((state & TupleFieldState.Available) == 0)
        throw new InvalidOperationException(Strings.ExValueIsNotAvailable);
      return (state & TupleFieldState.Null) > 0;
    }

    /// <inheritdoc/>
    public bool HasValue(int fieldIndex)
    {
      return GetFieldState(fieldIndex) == TupleFieldState.Available;
    }

    #endregion

    #region GetFieldState (abstract), GetValueOrDefault (asbtract), GetValue, SetValue methods

    /// <inheritdoc />
    public abstract TupleFieldState GetFieldState(int fieldIndex);

    /// <inheritdoc/>
    public abstract object GetValueOrDefault(int fieldIndex);

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Field value is not available.</exception>
    public object GetValue(int fieldIndex)
    {
      if (!IsAvailable(fieldIndex))
        throw new InvalidOperationException(Strings.ExValueIsNotAvailable);
      return GetValueOrDefault(fieldIndex);
    }

    /// <inheritdoc />
    public abstract void SetValue(int fieldIndex, object fieldValue);

    #endregion

    #region GetValueOrDefault<T>, GetValue<T>, SetValue<T> methods

    /// <summary>
    /// Gets the value field value by its index, if it is available;
    /// otherwise returns <see langword="default(T)"/>.
    /// </summary>
    /// <param name="fieldIndex">Index of the field to get value of.</param>
    /// <returns>Field value, if it is available;
    /// otherwise, <see langword="default(T)"/>.</returns>
    /// <typeparam name="T">The type of value to get.</typeparam>
    /// <exception cref="InvalidCastException">Value is available, but it can't be cast
    /// to specified type. E.g. if value is <see langword="null"/>, field is struct, 
    /// but <typeparamref name="T"/> is not a <see cref="Nullable{T}"/> type.</exception>
    public T GetValueOrDefault<T>(int fieldIndex)
    {
      var getter = (null == default(T) // Is nullable value type or class
        ? GetGetNullableValueOrDefaultDelegate(fieldIndex)
        : GetGetValueOrDefaultDelegate(fieldIndex)) as Func<Tuple, T>;
      if (getter != null)
        return getter.Invoke(this);
      return (T) GetValueOrDefault(fieldIndex);
    }

    /// <summary>
    /// Gets the value field value by its index.
    /// </summary>
    /// <param name="fieldIndex">Index of the field to get value of.</param>
    /// <returns>Field value.</returns>
    /// <typeparam name="T">The type of value to get.</typeparam>
    /// <remarks>
    /// If field value is not available (see <see cref="Tuple.IsAvailable"/>),
    /// an exception will be thrown.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Field value is not available.</exception>
    /// <exception cref="InvalidCastException">Value is available, but it can't be cast
    /// to specified type. E.g. if value is <see langword="null"/>, field is struct, 
    /// but <typeparamref name="T"/> is not a <see cref="Nullable{T}"/> type.</exception>
    public T GetValue<T>(int fieldIndex)
    {
      if (!IsAvailable(fieldIndex))
        throw new InvalidOperationException(Strings.ExValueIsNotAvailable);

      var getter = (null == default(T) // Is nullable value type or class
        ? GetGetNullableValueOrDefaultDelegate(fieldIndex)
        : GetGetValueOrDefaultDelegate(fieldIndex)) as Func<Tuple, T>;
      if (getter != null)
        return getter.Invoke(this);
      return (T) GetValueOrDefault(fieldIndex);
    }

    /// <summary>
    /// Sets the field value by its index.
    /// </summary>
    /// <param name="fieldIndex">Index of the field to set value of.</param>
    /// <param name="fieldValue">Field value.</param>
    /// <typeparam name="T">The type of value to set.</typeparam>
    /// <exception cref="InvalidCastException">Type of stored value and <typeparamref name="T"/>
    /// are incompatible.</exception>
    public void SetValue<T>(int fieldIndex, T fieldValue)
    {
      var setter = (null == default(T) // Is nullable value type or class
        ? GetSetNullableValueDelegate(fieldIndex)
        : GetSetValueDelegate(fieldIndex)) as Action<Tuple, T>;
      if (setter != null)
        setter.Invoke(this, fieldValue);
      else
        SetValue(fieldIndex, (object) fieldValue);
    }

    #endregion

    #region Get Delegate methods

    protected virtual Delegate GetGetValueOrDefaultDelegate(int fieldIndex)
    {
      return null;
    }

    protected virtual Delegate GetGetNullableValueOrDefaultDelegate(int fieldIndex)
    {
      return null;
    }

    protected virtual Delegate GetSetValueDelegate(int fieldIndex)
    {
      return null;
    }

    protected virtual Delegate GetSetNullableValueDelegate(int fieldIndex)
    {
      return null;
    }

    #endregion
    
    #region IComparable, IEquatable

    /// <inheritdoc/>
    public int CompareTo(Tuple other)
    {
      return AdvancedComparerStruct<Tuple>.System.Compare(this, other);
    }

    /// <inheritdoc/>
    public int CompareTo(ITuple other)
    {
      return CompareTo((Tuple)other);
    }

    /// <inheritdoc/>
    public virtual bool Equals(Tuple other)
    {
      return AdvancedComparerStruct<Tuple>.System.Equals(this, other);
    }

    /// <inheritdoc/>
    public bool Equals(ITuple other)
    {
      return Equals((Tuple) other);
    }

    #endregion

    #region Equals, GetHashCode

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      return Equals(obj as Tuple);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      var count = Count;
      int result = 0;
      for (int i = 0; i<count; i++) {
        object valueOrDefault = GetValueOrDefault(i);
        result = HashCodeMultiplier * result ^ (valueOrDefault != null ? valueOrDefault.GetHashCode() : 0);
      }
      return result;
    }

    #endregion

    #region ToString methods

    /// <inheritdoc/>
    public override string ToString()
    {
      var sb = new StringBuilder(16);
      for (int i = 0; i < Count; i++) {
        if (i > 0)
          sb.Append(", ");
        if (!IsAvailable(i))
          sb.Append(Strings.NotAvailable);
        else if (IsNull(i))
          sb.Append(Strings.Null);
        else if (Descriptor[i] == typeof(string)) {
          var value = GetValue(i) as string;
          if (string.IsNullOrEmpty(value))
            sb.Append(Strings.EmptyString);
          else
            sb.Append(value);
        }
        else
          sb.Append(GetValue(i));
      }
      return string.Format(Strings.TupleFormat, sb);
    }

    #endregion

    #region Create methods (base)

    /// <summary>
    /// Creates new <see cref="Tuple"/> by its field types.
    /// </summary>
    /// <param name="fieldTypes">Array of field types.</param>
    /// <returns>Newly created <see cref="RegularTuple"/> object.</returns>
    public static RegularTuple Create(params Type[] fieldTypes)
    {
      TupleDescriptor descriptor = TupleDescriptor.Create(fieldTypes);
      return Create(descriptor);
    }

    /// <summary>
    /// Creates new <see cref="Tuple"/> by its descriptor.
    /// </summary>
    /// <param name="descriptor">The descriptor.</param>
    /// <returns>Newly created <see cref="RegularTuple"/> object.</returns>
    public static RegularTuple Create(TupleDescriptor descriptor)
    {
      descriptor.EnsureIsCompiled();
      return (RegularTuple)descriptor.TupleFactory.CreateNew();
    }

    #endregion

    #region Create<T1,T2, ...> methods 

    /// <summary>
    /// Creates new <see cref="Tuple"/> by its field value(s).
    /// </summary>
    /// <typeparam name="T">Type of the only tuple field.</typeparam>
    /// <param name="value">Value of the only tuple field.</param>
    /// <returns>Newly created <see cref="RegularTuple"/> object.</returns>
    public static RegularTuple Create<T>(T value)
    {
      TupleDescriptor descriptor = TupleDescriptor.Create(new[] {
        typeof(T)
      });
      return Create(descriptor, value);
    }

    /// <summary>
    /// Creates new <see cref="Tuple"/> by its field value(s).
    /// </summary>
    /// <typeparam name="T">Type of the only tuple field.</typeparam>
    /// <param name="descriptor">Tuple descriptor.</param>
    /// <param name="value">Value of the only tuple field.</param>
    /// <returns>Newly created <see cref="RegularTuple"/> object.</returns>
    public static RegularTuple Create<T>(TupleDescriptor descriptor, T value)
    {
      RegularTuple tuple = Create(descriptor);
      tuple.SetValue(0, value);
      return tuple;
    }

    /// <summary>
    /// Creates new <see cref="Tuple"/> by its field value(s).
    /// </summary>
    /// <typeparam name="T1">Type of the first tuple field.</typeparam>
    /// <typeparam name="T2">Type of the 2nd tuple field.</typeparam>
    /// <param name="value1">Value of the first tuple field.</param>
    /// <param name="value2">Value of the 2nd tuple field.</param>
    /// <returns>Newly created <see cref="RegularTuple"/> object.</returns>
    public static RegularTuple Create<T1,T2>(T1 value1, T2 value2)
    {
      TupleDescriptor descriptor = TupleDescriptor.Create(new[] {
        typeof(T1),
        typeof(T2)
      });
      return Create(descriptor, value1, value2);
    }

    /// <summary>
    /// Creates new <see cref="Tuple"/> by its field value(s).
    /// </summary>
    /// <typeparam name="T1">Type of the first tuple field.</typeparam>
    /// <typeparam name="T2">Type of the 2nd tuple field.</typeparam>
    /// <param name="descriptor">Tuple descriptor.</param>
    /// <param name="value1">Value of the first tuple field.</param>
    /// <param name="value2">Value of the 2nd tuple field.</param>
    /// <returns>Newly created <see cref="RegularTuple"/> object.</returns>
    public static RegularTuple Create<T1,T2>(TupleDescriptor descriptor, T1 value1, T2 value2)
    {
      RegularTuple tuple = Create(descriptor);
      tuple.SetValue(0, value1);
      tuple.SetValue(1, value2);
      return tuple;
    }

    /// <summary>
    /// Creates new <see cref="Tuple"/> by its field value(s).
    /// </summary>
    /// <typeparam name="T1">Type of the first tuple field.</typeparam>
    /// <typeparam name="T2">Type of the 2nd tuple field.</typeparam>
    /// <typeparam name="T3">Type of the 3rd tuple field.</typeparam>
    /// <param name="value1">Value of the first tuple field.</param>
    /// <param name="value2">Value of the 2nd tuple field.</param>
    /// <param name="value3">Value of the 3rd tuple field.</param>
    /// <returns>Newly created <see cref="RegularTuple"/> object.</returns>
    public static RegularTuple Create<T1,T2,T3>(T1 value1, T2 value2, T3 value3)
    {
      TupleDescriptor descriptor = TupleDescriptor.Create(new[] {
        typeof(T1),
        typeof(T2),
        typeof(T3)
      });
      return Create(descriptor, value1, value2, value3);
    }

    /// <summary>
    /// Creates new <see cref="Tuple"/> by its field value(s).
    /// </summary>
    /// <typeparam name="T1">Type of the first tuple field.</typeparam>
    /// <typeparam name="T2">Type of the 2nd tuple field.</typeparam>
    /// <typeparam name="T3">Type of the 3rd tuple field.</typeparam>
    /// <param name="descriptor">Tuple descriptor.</param>
    /// <param name="value1">Value of the first tuple field.</param>
    /// <param name="value2">Value of the 2nd tuple field.</param>
    /// <param name="value3">Value of the 3rd tuple field.</param>
    /// <returns>Newly created <see cref="RegularTuple"/> object.</returns>
    public static RegularTuple Create<T1, T2, T3>(TupleDescriptor descriptor, T1 value1, T2 value2, T3 value3)
    {
      RegularTuple tuple = Create(descriptor);
      tuple.SetValue(0, value1);
      tuple.SetValue(1, value2);
      tuple.SetValue(2, value3);
      return tuple;
    }

    /// <summary>
    /// Creates new <see cref="Tuple"/> by its field value(s).
    /// </summary>
    /// <typeparam name="T1">Type of the first tuple field.</typeparam>
    /// <typeparam name="T2">Type of the 2nd tuple field.</typeparam>
    /// <typeparam name="T3">Type of the 3rd tuple field.</typeparam>
    /// <typeparam name="T4">Type of the 4th tuple field.</typeparam>
    /// <param name="value1">Value of the first tuple field.</param>
    /// <param name="value2">Value of the 2nd tuple field.</param>
    /// <param name="value3">Value of the 3rd tuple field.</param>
    /// <param name="value4">Value of the 4th tuple field.</param>
    /// <returns>Newly created <see cref="RegularTuple"/> object.</returns>
    public static RegularTuple Create<T1,T2,T3,T4>(T1 value1, T2 value2, T3 value3, T4 value4)
    {
      TupleDescriptor descriptor = TupleDescriptor.Create(new[] {
        typeof(T1),
        typeof(T2),
        typeof(T3),
        typeof(T4)
      });
      return Create(descriptor, value1, value2, value3, value4);
    }

    /// <summary>
    /// Creates new <see cref="Tuple"/> by its field value(s).
    /// </summary>
    /// <typeparam name="T1">Type of the first tuple field.</typeparam>
    /// <typeparam name="T2">Type of the 2nd tuple field.</typeparam>
    /// <typeparam name="T3">Type of the 3rd tuple field.</typeparam>
    /// <typeparam name="T4">Type of the 4th tuple field.</typeparam>
    /// <param name="descriptor">Tuple descriptor.</param>
    /// <param name="value1">Value of the first tuple field.</param>
    /// <param name="value2">Value of the 2nd tuple field.</param>
    /// <param name="value3">Value of the 3rd tuple field.</param>
    /// <param name="value4">Value of the 4th tuple field.</param>
    /// <returns>Newly created <see cref="RegularTuple"/> object.</returns>
    public static RegularTuple Create<T1, T2, T3, T4>(TupleDescriptor descriptor, T1 value1, T2 value2, T3 value3, T4 value4)
    {
      RegularTuple tuple = Create(descriptor);
      tuple.SetValue(0, value1);
      tuple.SetValue(1, value2);
      tuple.SetValue(2, value3);
      tuple.SetValue(3, value4);
      return tuple;
    }

    /// <summary>
    /// Creates new <see cref="Tuple"/> by its field value(s).
    /// </summary>
    /// <typeparam name="T1">Type of the first tuple field.</typeparam>
    /// <typeparam name="T2">Type of the 2nd tuple field.</typeparam>
    /// <typeparam name="T3">Type of the 3rd tuple field.</typeparam>
    /// <typeparam name="T4">Type of the 4th tuple field.</typeparam>
    /// <typeparam name="T5">Type of the 5th tuple field.</typeparam>
    /// <param name="value1">Value of the first tuple field.</param>
    /// <param name="value2">Value of the 2nd tuple field.</param>
    /// <param name="value3">Value of the 3rd tuple field.</param>
    /// <param name="value4">Value of the 4th tuple field.</param>
    /// <param name="value5">Value of the 5th tuple field.</param>
    /// <returns>Newly created <see cref="RegularTuple"/> object.</returns>
    public static RegularTuple Create<T1,T2,T3,T4,T5>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
    {
      TupleDescriptor descriptor = TupleDescriptor.Create(new[] {
        typeof(T1),
        typeof(T2),
        typeof(T3),
        typeof(T4),
        typeof(T5)
      });
      return Create(descriptor, value1, value2, value3, value4, value5);
    }

    /// <summary>
    /// Creates new <see cref="Tuple"/> by its field value(s).
    /// </summary>
    /// <typeparam name="T1">Type of the first tuple field.</typeparam>
    /// <typeparam name="T2">Type of the 2nd tuple field.</typeparam>
    /// <typeparam name="T3">Type of the 3rd tuple field.</typeparam>
    /// <typeparam name="T4">Type of the 4th tuple field.</typeparam>
    /// <typeparam name="T5">Type of the 5th tuple field.</typeparam>
    /// <param name="descriptor">Tuple descriptor.</param>
    /// <param name="value1">Value of the first tuple field.</param>
    /// <param name="value2">Value of the 2nd tuple field.</param>
    /// <param name="value3">Value of the 3rd tuple field.</param>
    /// <param name="value4">Value of the 4th tuple field.</param>
    /// <param name="value5">Value of the 5th tuple field.</param>
    /// <returns>Newly created <see cref="RegularTuple"/> object.</returns>
    public static RegularTuple Create<T1, T2, T3, T4, T5>(TupleDescriptor descriptor, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
    {
      RegularTuple tuple = Create(descriptor);
      tuple.SetValue(0, value1);
      tuple.SetValue(1, value2);
      tuple.SetValue(2, value3);
      tuple.SetValue(3, value4);
      tuple.SetValue(4, value5);
      return tuple;
    }

    /// <summary>
    /// Creates new <see cref="Tuple"/> by its field value(s).
    /// </summary>
    /// <typeparam name="T1">Type of the first tuple field.</typeparam>
    /// <typeparam name="T2">Type of the 2nd tuple field.</typeparam>
    /// <typeparam name="T3">Type of the 3rd tuple field.</typeparam>
    /// <typeparam name="T4">Type of the 4th tuple field.</typeparam>
    /// <typeparam name="T5">Type of the 5th tuple field.</typeparam>
    /// <typeparam name="T6">Type of the 6th tuple field.</typeparam>
    /// <param name="value1">Value of the first tuple field.</param>
    /// <param name="value2">Value of the 2nd tuple field.</param>
    /// <param name="value3">Value of the 3rd tuple field.</param>
    /// <param name="value4">Value of the 4th tuple field.</param>
    /// <param name="value5">Value of the 5th tuple field.</param>
    /// <param name="value6">Value of the 6th tuple field.</param>
    /// <returns>Newly created <see cref="RegularTuple"/> object.</returns>
    public static RegularTuple Create<T1,T2,T3,T4,T5,T6>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6)
    {
      TupleDescriptor descriptor = TupleDescriptor.Create(new[] {
        typeof(T1),
        typeof(T2),
        typeof(T3),
        typeof(T4),
        typeof(T5),
        typeof(T6)
      });
      return Create(descriptor, value1, value2, value3, value4, value5, value6);
    }

    /// <summary>
    /// Creates new <see cref="Tuple"/> by its field value(s).
    /// </summary>
    /// <typeparam name="T1">Type of the first tuple field.</typeparam>
    /// <typeparam name="T2">Type of the 2nd tuple field.</typeparam>
    /// <typeparam name="T3">Type of the 3rd tuple field.</typeparam>
    /// <typeparam name="T4">Type of the 4th tuple field.</typeparam>
    /// <typeparam name="T5">Type of the 5th tuple field.</typeparam>
    /// <typeparam name="T6">Type of the 6th tuple field.</typeparam>
    /// <param name="descriptor">Tuple descriptor.</param>
    /// <param name="value1">Value of the first tuple field.</param>
    /// <param name="value2">Value of the 2nd tuple field.</param>
    /// <param name="value3">Value of the 3rd tuple field.</param>
    /// <param name="value4">Value of the 4th tuple field.</param>
    /// <param name="value5">Value of the 5th tuple field.</param>
    /// <param name="value6">Value of the 6th tuple field.</param>
    /// <returns>Newly created <see cref="RegularTuple"/> object.</returns>
    public static RegularTuple Create<T1, T2, T3, T4, T5, T6>(TupleDescriptor descriptor, T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6)
    {
      RegularTuple tuple = Create(descriptor);
      tuple.SetValue(0, value1);
      tuple.SetValue(1, value2);
      tuple.SetValue(2, value3);
      tuple.SetValue(3, value4);
      tuple.SetValue(4, value5);
      tuple.SetValue(5, value6);
      return tuple;
    }

    #endregion

    /// <see cref="TupleFormatExtensions.Parse" copy="true" />
    public static Tuple Parse(TupleDescriptor descriptor, string source)
    {
      return descriptor.Parse(source);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    protected Tuple()
    {
    }
  }
}

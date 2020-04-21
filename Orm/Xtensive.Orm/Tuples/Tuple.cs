// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.05.30

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using Xtensive.Comparison;
using Xtensive.Core;


using Xtensive.Tuples.Packed;

namespace Xtensive.Tuples
{
  /// <summary>
  /// A base class for auto generated tuples.
  /// </summary>
  [DataContract]
  [Serializable]
  public abstract class Tuple : ITuple, IEquatable<Tuple>
  {
    /// <summary>
    /// Per-field hash code multiplier used in <see cref="GetHashCode"/> calculation.
    /// </summary>
    public const int HashCodeMultiplier = 397;

    /// <inheritdoc />
    [IgnoreDataMember]
    public abstract TupleDescriptor Descriptor { get; }

    /// <inheritdoc />
    [IgnoreDataMember]
    public virtual int Count
    {
      [DebuggerStepThrough]
      get { return Descriptor.Count; }
    }

    /// <inheritdoc/>
    Tuple ITupleFactory.CreateNew()
    {
      return CreateNew();
    }

    /// <inheritdoc/>
    Tuple ITuple.Clone()
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

    /// <inheritdoc />
    public abstract TupleFieldState GetFieldState(int fieldIndex);

    protected internal abstract void SetFieldState(int fieldIndex, TupleFieldState fieldState);

    /// <inheritdoc/>
    public abstract object GetValue(int fieldIndex, out TupleFieldState fieldState);

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Field value is not available.</exception>
    public object GetValue(int fieldIndex)
    {
      TupleFieldState state;
      var result = GetValue(fieldIndex, out state);
      return state.IsNull() ? null : result;
    }

    /// <inheritdoc/>
    public object GetValueOrDefault(int fieldIndex)
    {
      TupleFieldState state;
      var value = GetValue(fieldIndex, out state);
      return state==TupleFieldState.Available ? value : null;
    }

    /// <inheritdoc />
    public abstract void SetValue(int fieldIndex, object fieldValue);

    /// <summary>
    /// Gets the value field value by its index, if it is available;
    /// otherwise returns <see langword="default(T)"/>.
    /// </summary>
    /// <param name="fieldIndex">Index of the field to get value of.</param>
    /// <param name="fieldState">Field state associated with the field.</param>
    /// <returns>Field value, if it is available; otherwise, <see langword="default(T)"/>.</returns>
    /// <typeparam name="T">The type of value to get.</typeparam>
    public T GetValue<T>(int fieldIndex, out TupleFieldState fieldState)
    {
      var isNullable = null==default(T); // Is nullable value type or class

      if (this is PackedTuple packedTuple) {
        ref var descriptor = ref packedTuple.PackedDescriptor.FieldDescriptors[fieldIndex];
        return descriptor.Accessor.GetValue<T>(packedTuple, ref descriptor, isNullable, out fieldState);
      }

      var mappedContainer = GetMappedContainer(fieldIndex, false);
      if (mappedContainer.First is PackedTuple mappedTuple) {
        ref var descriptor = ref mappedTuple.PackedDescriptor.FieldDescriptors[mappedContainer.Second];
        return descriptor.Accessor.GetValue<T>(mappedTuple, ref descriptor, isNullable, out fieldState);
      }

      var value = GetValue(fieldIndex, out fieldState);
      return value!=null ? (T) value : default(T);
    }

    /// <summary>
    /// Gets the value field value by its index.
    /// </summary>
    /// <param name="fieldIndex">Index of the field to get value of.</param>
    /// <returns>Field value.</returns>
    /// <typeparam name="T">The type of value to get.</typeparam>
    /// <remarks>
    /// If field value is not available (see <see cref="TupleFieldState.Available"/>),
    /// an exception will be thrown.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Field value is not available.</exception>
    /// <exception cref="InvalidCastException">Value is available, but it can't be cast
    /// to specified type. E.g. if value is <see langword="null"/>, field is struct, 
    /// but <typeparamref name="T"/> is not a <see cref="Nullable{T}"/> type.</exception>
    public T GetValue<T>(int fieldIndex)
    {
      TupleFieldState fieldState;
      var result = GetValue<T>(fieldIndex, out fieldState);

      if (fieldState.IsNull()) {
        if (default(T)!=null)
          throw new InvalidCastException(string.Format(Strings.ExUnableToCastNullValueToXUseXInstead, typeof (T)));
        return default(T);
      }

      return result;
    }

    /// <summary>
    /// Gets the value field value by its index.
    /// </summary>
    /// <param name="fieldIndex">Index of the field to get value of.</param>
    /// <returns>Field value.</returns>
    /// <typeparam name="T">The type of value to get.</typeparam>
    /// <remarks>
    /// If field value is not available (see <see cref="TupleFieldState.Available"/>),
    /// an exception will be thrown.
    /// </remarks>
    /// <exception cref="InvalidCastException">Value is available, but it can't be cast
    /// to specified type. E.g. if value is <see langword="null"/>, field is struct, 
    /// but <typeparamref name="T"/> is not a <see cref="Nullable{T}"/> type.</exception>
    public T GetValueOrDefault<T>(int fieldIndex)
    {
      TupleFieldState fieldState;
      var result = GetValue<T>(fieldIndex, out fieldState);
      return fieldState==TupleFieldState.Available ? result : default(T);
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
      var isNullable = null==default(T); // Is nullable value type or class

      if (this is PackedTuple packedTuple) {
        ref var descriptor = ref packedTuple.PackedDescriptor.FieldDescriptors[fieldIndex];
        descriptor.Accessor.SetValue(packedTuple, ref descriptor, isNullable, fieldValue);
        return;
      }

      var mappedContainer = GetMappedContainer(fieldIndex, true);
      if (mappedContainer.First is PackedTuple mappedTuple) {
        ref var descriptor = ref mappedTuple.PackedDescriptor.FieldDescriptors[mappedContainer.Second];
        descriptor.Accessor.SetValue(mappedTuple, ref descriptor, isNullable, fieldValue);
        return;
      }

      SetValue(fieldIndex, (object) fieldValue);
    }

    /// <summary>
    /// Gets the tuple containing actual value of the specified field.
    /// </summary>
    /// <param name="fieldIndex">Index of the field to get the value container for.</param>
    /// <param name="isWriting">Indicates whether method caller has a writing intention.</param>
    /// <returns>Tuple container and remapped field index.</returns>
    protected internal virtual Pair<Tuple, int> GetMappedContainer(int fieldIndex, bool isWriting)
    {
      return new Pair<Tuple, int>(this, fieldIndex);
    }

    #region Equals, GetHashCode

    /// <inheritdoc/>
    public override sealed bool Equals(object obj)
    {
      return Equals(obj as Tuple);
    }

    /// <inheritdoc/>
    public virtual bool Equals(Tuple other)
    {
      if (ReferenceEquals(other, null))
        return false;
      if (ReferenceEquals(other, this))
        return true;
      if (Descriptor!=other.Descriptor)
        return false;

      var count = Count;
      for (int i = 0; i < count; i++) {
        TupleFieldState thisState;
        TupleFieldState otherState;
        var thisValue = GetValue(i, out thisState);
        var otherValue = other.GetValue(i, out otherState);
        if (thisState!=otherState)
          return false;
        if (thisState!=TupleFieldState.Available)
          continue;
        if (!Equals(thisValue, otherValue))
          return false;
      }

      return true;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      var count = Count;
      int result = 0;
      for (int i = 0; i < count; i++) {
        TupleFieldState state;
        object value = GetValue(i, out state);
        result = HashCodeMultiplier * result ^ (value!=null ? value.GetHashCode() : 0);
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
        TupleFieldState state;
        var value = GetValue(i, out state);
        if (i > 0)
          sb.Append(", ");
        if (!state.IsAvailable())
          sb.Append(Strings.NotAvailable);
        else if (state.IsNull())
          sb.Append(Strings.Null);
        else if (Descriptor[i]==typeof (string)) {
          if (string.IsNullOrEmpty(value as string))
            sb.Append(Strings.EmptyString);
          else
            sb.Append(value);
        }
        else
          sb.Append(value);
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
      if (descriptor==null)
        throw new ArgumentNullException("descriptor");
      return new PackedTuple(descriptor);
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
        typeof (T)
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
    public static RegularTuple Create<T1, T2>(T1 value1, T2 value2)
    {
      TupleDescriptor descriptor = TupleDescriptor.Create(new[] {
        typeof (T1),
        typeof (T2)
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
    public static RegularTuple Create<T1, T2>(TupleDescriptor descriptor, T1 value1, T2 value2)
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
    public static RegularTuple Create<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
    {
      TupleDescriptor descriptor = TupleDescriptor.Create(new[] {
        typeof (T1),
        typeof (T2),
        typeof (T3)
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
    public static RegularTuple Create<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4)
    {
      TupleDescriptor descriptor = TupleDescriptor.Create(new[] {
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4)
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
    public static RegularTuple Create<T1, T2, T3, T4, T5>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
    {
      TupleDescriptor descriptor = TupleDescriptor.Create(new[] {
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4),
        typeof (T5)
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
    public static RegularTuple Create<T1, T2, T3, T4, T5, T6>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6)
    {
      TupleDescriptor descriptor = TupleDescriptor.Create(new[] {
        typeof (T1),
        typeof (T2),
        typeof (T3),
        typeof (T4),
        typeof (T5),
        typeof (T6)
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
    /// Initializes a new instance of this type.
    /// </summary>
    protected Tuple()
    {
    }
  }
}

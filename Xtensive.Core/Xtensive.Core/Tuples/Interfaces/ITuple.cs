// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.05.30

using System;

namespace Xtensive.Tuples
{
  /// <summary>
  /// Describes a tuple.
  /// </summary>
  public interface ITuple : 
    ICloneable,
    ITupleFactory
  {
    /// <summary>
    /// Gets tuple descriptor.
    /// </summary>
    TupleDescriptor Descriptor { get; }

    /// <summary>
    /// Gets field count for this instance.
    /// </summary>
    int Count { get;}

    /// <summary>
    /// Clones the tuple.
    /// </summary>
    /// <returns>A new instance of the tuple of the same type
    /// and with the same field values.</returns>
    new Tuple Clone();

    /// <summary>
    /// Gets the field state associated with the field.
    /// </summary>
    /// <param name="fieldIndex">Index of the field to get the state for.</param>
    TupleFieldState GetFieldState(int fieldIndex);

    /// <summary>
    /// Gets the field value by its index.
    /// </summary>
    /// <param name="fieldIndex">Index of the field to get value of.</param>
    /// <returns>Field value.</returns>
    /// <remarks>
    /// If field value is not available (see <see cref="TupleFieldState.Available"/>),
    /// an exception will be thrown.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Field value is not available.</exception>
    object GetValue(int fieldIndex);

    /// <summary>
    /// Gets the field value by its index, if it is available;
    /// otherwise returns default value for field type.
    /// </summary>
    /// <param name="fieldIndex">Index of the field to get value of.</param>
    /// <param name="fieldState">Field state associated with the field.</param>
    /// <returns>Field value, if it is available; otherwise, default value for field type.</returns>
    object GetValue(int fieldIndex, out TupleFieldState fieldState);

    /// <summary>
    /// Gets the value field value by its index, if it is available (see <see cref="TupleFieldState.Available"/>) and is not null (see <see cref="TupleFieldState.Null"/>);
    /// otherwise returns <see langword="null" />.
    /// </summary>
    /// <param name="fieldIndex">Index of the field to get value of.</param>
    /// <returns>Field value, if it is available and not null; otherwise, <see langword="null" />.</returns>
    object GetValueOrDefault(int fieldIndex);

    /// <summary>
    /// Sets the field value by its index.
    /// </summary>
    /// <param name="fieldIndex">Index of the field to set value of.</param>
    /// <param name="fieldValue">Field value.</param>
    /// <exception cref="InvalidCastException">Type of stored value is incompatible
    /// with the specified one.</exception>
    void SetValue(int fieldIndex, object fieldValue);
  }
}

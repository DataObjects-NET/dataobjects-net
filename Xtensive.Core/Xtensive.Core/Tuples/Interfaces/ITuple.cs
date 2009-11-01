// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.05.30

using System;

namespace Xtensive.Core.Tuples
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
    /// Gets or sets tuple field by its index.
    /// </summary>
    /// <param name="fieldIndex">Index of field to set or get.</param>
    /// <returns>Field value.</returns>
    object this[int fieldIndex] { get; set; }

    #region Clone method

    /// <summary>
    /// Clones the tuple.
    /// </summary>
    /// <returns>A new instance of the tuple of the same type
    /// and with the same field values.</returns>
    new ITuple Clone();

    #endregion

    #region IsXxx, HasXxx, GetFieldState methods

    /// <summary>
    /// Checks if specified field is available.
    /// </summary>
    /// <param name="fieldIndex">Index of the field to check.</param>
    /// <returns><see langword="True"/> if field value is available;
    /// otherwise, <see langword="false"/>.</returns>
    bool IsAvailable(int fieldIndex);

    /// <summary>
    /// Checks if specified field has <see langword="null"/> value.
    /// </summary>
    /// <param name="fieldIndex">Index of the field to check.</param>
    /// <returns><see langword="True"/> if field value is <see langword="null"/>;
    /// otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This method works both for <see langword="struct"/> and 
    /// <see langword="class"/>-type fields. Its call is equal to evaluation of 
    /// the following expression: 
    /// <code language="C#">
    ///   !IsAvailable(fieldIndex) || GetFlag(fieldIndex, TupleFieldFlag.IsNull)
    /// </code>
    /// </remarks>
    bool IsNull(int fieldIndex);

    /// <summary>
    /// Checks if specified field is available and has non-<see langword="null"/> value.
    /// </summary>
    /// <param name="fieldIndex">Index of the field to check.</param>
    /// <returns><see langword="True"/> if field value is available is not <see langword="null"/>;
    /// otherwise, <see langword="false"/>.</returns>
    bool HasValue(int fieldIndex);

    /// <summary>
    /// Gets the field state associated with the field.
    /// </summary>
    /// <param name="fieldIndex">Index of the field to get the state for.</param>
    TupleFieldState GetFieldState(int fieldIndex);

    #endregion

    #region GetValue, GetValueOrDefault methods

    /// <summary>
    /// Gets the value field value by its index.
    /// </summary>
    /// <param name="fieldIndex">Index of the field to get value of.</param>
    /// <returns>Field value.</returns>
    /// <typeparam name="T">The type of value to get.</typeparam>
    /// <remarks>
    /// If field value is not available (see <see cref="IsAvailable"/>),
    /// an exception will be thrown.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Field value is not available.</exception>
    /// <exception cref="InvalidCastException">Value is available, but it can't be cast
    /// to specified type. E.g. if value is <see langword="null"/>, field is struct, 
    /// but <typeparamref name="T"/> is not a <see cref="Nullable{T}"/> type.</exception>
    T GetValue<T>(int fieldIndex);

    /// <summary>
    /// Gets the value field value by its index.
    /// </summary>
    /// <param name="fieldIndex">Index of the field to get value of.</param>
    /// <returns>Field value.</returns>
    /// <remarks>
    /// If field value is not available (see <see cref="IsAvailable"/>),
    /// an exception will be thrown.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Field value is not available.</exception>
    object GetValue(int fieldIndex);

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
    T GetValueOrDefault<T>(int fieldIndex);

    /// <summary>
    /// Gets the value field value by its index, if it is available;
    /// otherwise returns <see langword="default(T)"/>.
    /// </summary>
    /// <param name="fieldIndex">Index of the field to get value of.</param>
    /// <returns>Field value, if it is available;
    /// otherwise, <see langword="default(T)"/>.</returns>
    object GetValueOrDefault(int fieldIndex);

    #endregion

    #region SetValue methods

    /// <summary>
    /// Sets the field value by its index.
    /// </summary>
    /// <param name="fieldIndex">Index of the field to set value of.</param>
    /// <param name="fieldValue">Field value.</param>
    /// <typeparam name="T">The type of value to set.</typeparam>
    /// <exception cref="InvalidCastException">Type of stored value and <typeparamref name="T"/>
    /// are incompatible.</exception>
    void SetValue<T>(int fieldIndex, T fieldValue);

    /// <summary>
    /// Sets the field value by its index.
    /// </summary>
    /// <param name="fieldIndex">Index of the field to set value of.</param>
    /// <param name="fieldValue">Field value.</param>
    /// <exception cref="InvalidCastException">Type of stored value is incompatible
    /// with the specified one.</exception>
    void SetValue(int fieldIndex, object fieldValue);

    #endregion
  }
}

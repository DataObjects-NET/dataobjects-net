// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.05.30

using System;

namespace Xtensive.Core.Tuples
{
  /// <summary>
  /// Describes a tuple with at least one field of 
  /// specified type <typeparamref name="T"/>.
  /// </summary>
  /// <typeparam name="T">Type of field that exists in the tuple.</typeparam>
  public interface ITupleFieldAccessor<T>
  {
    /// <summary>
    /// Gets the value field value by its index, if it is available;
    /// otherwise returns <see langword="default(T)"/>.
    /// </summary>
    /// <param name="fieldIndex">Index of the field to get value of.</param>
    /// <returns>Field value, if it is available;
    /// otherwise, <see langword="default(T)"/>.</returns>
    /// <exception cref="InvalidCastException">Value is available, but it can't be cast
    /// to specified type. E.g. if value is <see langword="null"/>, field is struct, 
    /// but <typeparamref name="T"/> is not a <see cref="Nullable{T}"/> type.</exception>
    T GetValueOrDefault(int fieldIndex);

    /// <summary>
    /// Sets the field value by its index.
    /// </summary>
    /// <param name="fieldIndex">Index of the field to set value of.</param>
    /// <param name="fieldValue">Field value.</param>
    /// <exception cref="InvalidCastException">Type of stored value and <typeparamref name="T"/>
    /// are incompatible.</exception>
    void SetValue(int fieldIndex, T fieldValue);
  }
}

// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.09.17

using Xtensive.Tuples.Packed;

namespace Xtensive.Tuples
{
  /// <summary>
  /// Incapsulates <see cref="Tuple.GetValue{T}(int, out Xtensive.Tuples.TupleFieldState)"/> method.
  /// </summary>
  /// <typeparam name="TValue">Type of a value.</typeparam>
  /// <param name="tuple">Tuple to use.</param>
  /// <param name="descriptor">Field descriptor.</param>
  /// <param name="fieldState">State of a field.</param>
  /// <returns></returns>
  internal delegate TValue GetValueDelegate<TValue>(PackedTuple tuple, ref PackedFieldDescriptor descriptor, out TupleFieldState fieldState);

  /// <summary>
  /// Incapsulates <see cref="Tuple.SetValue{T}"/> method.
  /// </summary>
  /// <typeparam name="TValue">Type of a value.</typeparam>
  /// <param name="tuple">Tuple to use.</param>
  /// <param name="descriptor">Field descriptor.</param>
  /// <param name="value">A value.</param>
  internal delegate void SetValueDelegate<TValue>(PackedTuple tuple, ref PackedFieldDescriptor descriptor, TValue value);
}
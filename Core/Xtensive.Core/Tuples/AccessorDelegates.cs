// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.09.17

namespace Xtensive.Tuples
{
  /// <summary>
  /// Incapsulates <see cref="Tuple.GetValue{T}(int, out Xtensive.Tuples.TupleFieldState)"/> method.
  /// </summary>
  /// <typeparam name="TValue">Type of a value.</typeparam>
  /// <param name="tuple">Tuple to use.</param>
  /// <param name="fieldIndex">Index of a field.</param>
  /// <param name="fieldState">State of a field.</param>
  /// <returns></returns>
  public delegate TValue GetValueDelegate<TValue>(Tuple tuple, int fieldIndex, out TupleFieldState fieldState);

  /// <summary>
  /// Incapsulates <see cref="Tuple.SetValue{T}"/> method.
  /// </summary>
  /// <typeparam name="TValue">Type of a value.</typeparam>
  /// <param name="tuple">Tuple to use.</param>
  /// <param name="fieldIndex">Index of a field.</param>
  /// <param name="value">A value.</param>
  public delegate void SetValueDelegate<TValue>(Tuple tuple, int fieldIndex, TValue value);
}
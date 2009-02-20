// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.20

namespace Xtensive.Core
{
  /// <summary>
  /// Encapsulates a method that has five parameters
  /// and returns a value of the type specified by the <see typeparamref="TResult"/> parameter.
  /// </summary>
  public delegate TResult Func<T1, T2, T3, T4, T5, TResult>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

  /// <summary>
  /// Encapsulates a method that has six parameters
  /// and returns a value of the type specified by the <see typeparamref="TResult"/> parameter.
  /// </summary>
  public delegate TResult Func<T1, T2, T3, T4, T5, T6, TResult>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);

  /// <summary>
  /// Encapsulates a method that has seven parameters
  /// and returns a value of the type specified by the <see typeparamref="TResult"/> parameter.
  /// </summary>
  public delegate TResult Func<T1, T2, T3, T4, T5, T6, T7, TResult>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);

  /// <summary>
  /// Encapsulates a method that has eight parameters
  /// and returns a value of the type specified by the <see typeparamref="TResult"/> parameter.
  /// </summary>
  public delegate TResult Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);

  /// <summary>
  /// Encapsulates a method that has nine parameters
  /// and returns a value of the type specified by the <see typeparamref="TResult"/> parameter.
  /// </summary>
  public delegate TResult Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(
    T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);
}

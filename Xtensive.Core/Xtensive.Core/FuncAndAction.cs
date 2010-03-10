// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.03.10

namespace Xtensive.Core
{
  /// <summary>
  /// Encapsulates a method that has 5 parameters
  /// and returns a value of the type specified by the <see typeparamref="TResult"/> parameter.
  /// </summary>
  public delegate TResult Func<T1, T2, T3, T4, T5, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

  /// <summary>
  /// Encapsulates a method that has 5 parameters and does not return a value.
  /// </summary>
  public delegate void Action<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

  /// <summary>
  /// Encapsulates a method that has 6 parameters
  /// and returns a value of the type specified by the <see typeparamref="TResult"/> parameter.
  /// </summary>
  public delegate TResult Func<T1, T2, T3, T4, T5, T6, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);

  /// <summary>
  /// Encapsulates a method that has 6 parameters and does not return a value.
  /// </summary>
  public delegate void Action<T1, T2, T3, T4, T5, T6>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);

  /// <summary>
  /// Encapsulates a method that has 7 parameters
  /// and returns a value of the type specified by the <see typeparamref="TResult"/> parameter.
  /// </summary>
  public delegate TResult Func<T1, T2, T3, T4, T5, T6, T7, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);

  /// <summary>
  /// Encapsulates a method that has 7 parameters and does not return a value.
  /// </summary>
  public delegate void Action<T1, T2, T3, T4, T5, T6, T7>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);

  /// <summary>
  /// Encapsulates a method that has 8 parameters
  /// and returns a value of the type specified by the <see typeparamref="TResult"/> parameter.
  /// </summary>
  public delegate TResult Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);

  /// <summary>
  /// Encapsulates a method that has 8 parameters and does not return a value.
  /// </summary>
  public delegate void Action<T1, T2, T3, T4, T5, T6, T7, T8>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);

  /// <summary>
  /// Encapsulates a method that has 9 parameters
  /// and returns a value of the type specified by the <see typeparamref="TResult"/> parameter.
  /// </summary>
  public delegate TResult Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);

  /// <summary>
  /// Encapsulates a method that has 9 parameters and does not return a value.
  /// </summary>
  public delegate void Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);

}
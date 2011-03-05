// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.09

namespace Xtensive.Reflection
{
  /// <summary>
  /// Execution sequence handler - a delegate that can be passed to 
  /// <see cref="DelegateHelper.ExecuteDelegates{T}"/> method.
  /// </summary>
  /// <typeparam name="T">Argument type.</typeparam>
  /// <param name="argument">Argument value.</param>
  /// <param name="index">Index of executed delegate in sequence.</param>
  /// <returns><see langword="True"/>, if execution of sequence should be stopped;
  /// otherwise, <see langword="false"/>.</returns>
  public delegate bool ExecutionSequenceHandler<T>(ref T argument, int index)
    where T : struct;
}
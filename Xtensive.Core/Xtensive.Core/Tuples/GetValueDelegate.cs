// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.09.17

using System;
using System.Diagnostics;

namespace Xtensive.Core.Tuples
{
  /// <summary>
  /// Incapsulates <see cref="Tuple.GetValue{T}(int,out Xtensive.Core.Tuples.TupleFieldState)" /> method.
  /// </summary>
  public delegate TResult GetValueDelegate<TResult>(Tuple tuple, out TupleFieldState fieldState);
}
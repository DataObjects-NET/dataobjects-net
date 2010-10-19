// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

namespace Xtensive.Arithmetic
{
  /// <summary>
  /// Arithmetic provider.
  /// </summary>
  public interface IArithmeticProvider
  {
    /// <summary>
    /// Gets <see cref="IArithmetic{T}"/> for the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type to get the arithmetic for.</typeparam>
    /// <returns><see cref="IArithmetic{T}"/> for the specified type <typeparamref name="T"/>.</returns>
    Arithmetic<T> GetArithmetic<T>();
  }
}
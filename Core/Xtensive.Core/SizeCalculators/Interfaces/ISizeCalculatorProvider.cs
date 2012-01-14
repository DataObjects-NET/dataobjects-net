// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using System;

namespace Xtensive.SizeCalculators
{
  /// <summary>
  /// Size calculator provider.
  /// </summary>
  public interface ISizeCalculatorProvider
  {
    /// <summary>
    /// Gets <see cref="SizeCalculator{T}"/> for the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type to get the size calculator for.</typeparam>
    /// <returns><see cref="ISizeCalculator{T}"/> for the specified type <typeparamref name="T"/>.</returns>
    SizeCalculator<T> GetSizeCalculator<T>();

    /// <summary>
    /// Gets <see cref="ISizeCalculatorBase"/> for the specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Value to get the size calculator for.</param>
    /// <returns><see cref="ISizeCalculatorBase"/> for the specified <paramref name="value"/>.</returns>
    ISizeCalculatorBase GetSizeCalculatorByInstance(object value);

    /// <summary>
    /// Gets <see cref="ISizeCalculatorBase"/> for the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type to get the size calculator for.</param>
    /// <returns><see cref="ISizeCalculatorBase"/> for the specified <paramref name="type"/>.</returns>
    ISizeCalculatorBase GetSizeCalculatorByType(Type type);
  }
}
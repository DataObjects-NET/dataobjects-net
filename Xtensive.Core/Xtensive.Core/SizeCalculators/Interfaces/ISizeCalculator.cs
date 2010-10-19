// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

namespace Xtensive.SizeCalculators
{
  /// <summary>
  /// Calculates the size of instances of type <typeparamref name="T"/>.
  /// </summary>
  /// <typeparam name="T">Type to calculate the size for.</typeparam>
  public interface ISizeCalculator<T>: ISizeCalculatorBase
  {
    /// <summary>
    /// Gets the size of the specified <paramref name="value"/> of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="value">Value to get the size for.</param>
    /// <returns>Size (in bytes) of specified <paramref name="value"/>.</returns>
    int GetValueSize(T value);
  }
}
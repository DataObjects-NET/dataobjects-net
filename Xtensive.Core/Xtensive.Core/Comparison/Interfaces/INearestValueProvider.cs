// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.02.01

using Xtensive.Core;

namespace Xtensive.Comparison
{
  /// <summary>
  /// Provides nearest values for the specified value of specified type <typeparamref name="T"/>.
  /// </summary>
  /// <typeparam name="T">The type of values.</typeparam>
  public interface INearestValueProvider<T>
  {
    /// <summary>
    /// Gets the nearest value in the specified direction.
    /// </summary>
    /// <param name="value">The initial value for which nearest value should be provided.</param>
    /// <param name="direction">The direction of the nearest value relative to <paramref name="value"/>.</param>
    T GetNearestValue(T value, Direction direction);
  }
}
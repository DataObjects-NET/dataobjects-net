// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.18

namespace Xtensive.Caching
{
  /// <summary>
  /// Cached value contract.
  /// </summary>
  /// <typeparam name="T">The type of the <see cref="Value"/>.</typeparam>
  public interface ICachedValue<T> : IInvalidatable
  {
    /// <summary>
    /// Gets the cached value.
    /// if it isn't actual (see <see cref="IsActual"/>),
    /// the value will be refreshed.
    /// </summary>
    T Value { get; }

    /// <summary>
    /// Gets a value indicating whether cached <see cref="Value"/> is actual,
    /// so an attempt to read it will not lead to refresh.
    /// </summary>
    bool IsActual { get; }
  }
}
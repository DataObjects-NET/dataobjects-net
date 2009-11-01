// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.28

using Xtensive.Core;

namespace Xtensive.Indexing
{
  internal interface IEntireFactory<T>
  {
    /// <summary>
    /// Creates the <see cref="IEntire{T}"/> for specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>Newly created <see cref="IEntire{T}"/> instance.</returns>
    IEntire<T> CreateEntire(T value);

    /// <summary>
    /// Creates the <see cref="IEntire{T}"/> for specified value.
    /// </summary>
    /// <param name="infinityType">Type of the infinity.</param>
    /// <returns>
    /// Newly created <see cref="IEntire{T}"/> instance.
    /// </returns>
    IEntire<T> CreateEntire(InfinityType infinityType);

    /// <summary>
    /// Creates the <see cref="IEntire{T}"/> for specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="infinitesimalShiftDirection">Infinitesimal shift direction.</param>
    /// <returns>
    /// Newly created <see cref="IEntire{T}"/> instance.
    /// </returns>
    IEntire<T> CreateEntire(T value, Direction infinitesimalShiftDirection);

    /// <summary>
    /// Creates the <see cref="IEntire{T}"/> for specified value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="fieldValueTypes">The field value types.</param>
    /// <returns>
    /// Newly created <see cref="IEntire{T}"/> instance.
    /// </returns>
    IEntire<T> CreateEntire(T value, params EntireValueType[] fieldValueTypes);
  }
}
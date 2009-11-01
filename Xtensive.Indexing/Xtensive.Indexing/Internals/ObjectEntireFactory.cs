// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.28

using Xtensive.Core;
using Xtensive.Core.Tuples;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Creates <see cref="IEntire{T}"/> for <see cref="Tuple"/> instances.
  /// </summary>
  internal class ObjectEntireFactory<T>: IEntireFactory<T>
  {
    /// <inheritdoc/>
    public IEntire<T> CreateEntire(T value)
    {
      return new Entire<T>(value);
    }

    /// <inheritdoc/>
    public IEntire<T> CreateEntire(InfinityType infinityType)
    {
      return new Entire<T>(infinityType);
    }

    /// <inheritdoc/>
    public IEntire<T> CreateEntire(T value, Direction infinitesimalShiftDirection)
    {
      return new Entire<T>(value, infinitesimalShiftDirection);
    }

    /// <inheritdoc/>
    public IEntire<T> CreateEntire(T value, params EntireValueType[] fieldValueTypes)
    {
      ArgumentValidator.EnsureArgumentNotNull(fieldValueTypes, "fieldValueTypes");
      ArgumentValidator.EnsureArgumentIsInRange(fieldValueTypes.Length, 1, 1, "fieldValueTypes");
      EntireValueType valueType = fieldValueTypes[0];
      switch(valueType) {
        case EntireValueType.NegativeInfinity:
          return CreateEntire(InfinityType.Negative);
        case EntireValueType.PositiveInfinity:
          return CreateEntire(InfinityType.Positive);
        case EntireValueType.NegativeInfinitesimal:
          return CreateEntire(value, Direction.Negative);
        case EntireValueType.PositiveInfinitesimal:
          return CreateEntire(value, Direction.Positive);
        default:
          return CreateEntire(value);
      }
    }
  }
}
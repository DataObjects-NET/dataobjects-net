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
  internal class TupleEntireFactory : IEntireFactory<Tuple>
  {
    /// <inheritdoc/>
    public IEntire<Tuple> CreateEntire(Tuple value)
    {
      return new TupleEntire(value);
    }

    /// <inheritdoc/>
    public IEntire<Tuple> CreateEntire(InfinityType infinityType)
    {
      return new TupleEntire(infinityType);
    }

    /// <inheritdoc/>
    public IEntire<Tuple> CreateEntire(Tuple value, Direction infinitesimalShiftDirection)
    {
      return new TupleEntire(value, infinitesimalShiftDirection);
    }

    /// <inheritdoc/>
    public IEntire<Tuple> CreateEntire(Tuple value, params EntireValueType[] fieldValueTypes)
    {
      return new TupleEntire(value, fieldValueTypes);
    }
  }
}
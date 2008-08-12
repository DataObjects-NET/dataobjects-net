// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.28

using System.Diagnostics;
using Xtensive.Core;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Creates <see cref="IEntire{T}"/> for <see cref="Pair{TFirst,TSecond}"/> instances.
  /// </summary>
  internal class PairEntireFactory<TFirst, TSecond> : IEntireFactory<Pair<TFirst, TSecond>>
  {
    /// <inheritdoc/>
    public IEntire<Pair<TFirst, TSecond>> CreateEntire(Pair<TFirst, TSecond> value)
    {
      return new PairEntire<TFirst, TSecond>(value);
    }

    /// <inheritdoc/>
    public IEntire<Pair<TFirst, TSecond>> CreateEntire(InfinityType infinityType)
    {
      return new PairEntire<TFirst, TSecond>(infinityType);
    }

    /// <inheritdoc/>
    public IEntire<Pair<TFirst, TSecond>> CreateEntire(Pair<TFirst, TSecond> value, Direction infinitesimalShiftDirection)
    {
      return new PairEntire<TFirst, TSecond>(value, infinitesimalShiftDirection);
    }

    /// <inheritdoc/>
    public IEntire<Pair<TFirst, TSecond>> CreateEntire(Pair<TFirst, TSecond> value, params EntireValueType[] fieldValueTypes)
    {
      return new PairEntire<TFirst, TSecond>(value, fieldValueTypes);
    }
  }
}
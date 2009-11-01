// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.28

using System;
using Xtensive.Core;
using Xtensive.Indexing.Composite;

namespace Xtensive.Indexing.Composite
{
  /// <summary>
  /// Creates <see cref="IEntire{T}"/> for <see cref="SegmentBound{T}"/> instances.
  /// </summary>
  internal class SegmentBoundEntireFactory<T> : IEntireFactory<SegmentBound<T>>
  {
    /// <inheritdoc/>
    public IEntire<SegmentBound<T>> CreateEntire(SegmentBound<T> value)
    {
      return new SegmentBoundEntire<T>(Entire<T>.Create(value.Value), value.SegmentNumber);
    }

    /// <inheritdoc/>
    public IEntire<SegmentBound<T>> CreateEntire(InfinityType infinityType)
    {
      return new SegmentBoundEntire<T>(Entire<T>.Create(infinityType), 0);
    }

    /// <inheritdoc/>
    public IEntire<SegmentBound<T>> CreateEntire(SegmentBound<T> value, Direction infinitesimalShiftDirection)
    {
      return new SegmentBoundEntire<T>(Entire<T>.Create(value.Value, infinitesimalShiftDirection), value.SegmentNumber);
    }

    /// <inheritdoc/>
    public IEntire<SegmentBound<T>> CreateEntire(SegmentBound<T> value, params EntireValueType[] fieldValueTypes)
    {
      return new SegmentBoundEntire<T>(Entire<T>.Create(value.Value, fieldValueTypes), value.SegmentNumber);
    }
  }
}
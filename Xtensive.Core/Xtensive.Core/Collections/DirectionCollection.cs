// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.09.24

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core.Conversion;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Collections
{
  ///<summary>
  /// A sequence of <typeparamref name="T"/>-<see cref="Direction"/> pairs.
  /// Normally used to describe "order by" clauses.
  ///</summary>
  /// <typeparam name="T">The type of collection item to associate with direction.</typeparam>
  [Serializable]
  [DebuggerDisplay("Count = {Count}")]
  public sealed class DirectionCollection<T>: FlagCollection<T, Direction>
  {
    /// <inheritdoc/>
    public override void Add(T key)
    {
      Add(key, Direction.Positive);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="enumerable">Initial content of collection.</param>
    public DirectionCollection(IEnumerable<KeyValuePair<T, Direction>> enumerable)
      : base(new Biconverter<Direction, bool>(
      delegate (Direction value) {
        if (value==Direction.None)
          throw Exceptions.InvalidArgument(value, "value");
        return value == Direction.Positive;
      },
      delegate(bool value) { return value ? Direction.Positive : Direction.Negative; }),
      enumerable)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="items">Initial content of collection.</param>
    public DirectionCollection(params T[] items)
      : base(new Biconverter<Direction, bool>(
        value => {
          if (value==Direction.None)
            throw Exceptions.InvalidArgument(value, "value");
          return value==Direction.Positive;
        },
        value => value ? Direction.Positive : Direction.Negative))
    {
      foreach (T item in items)
        Add(item, Direction.Positive);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    public DirectionCollection()
      : base(new Biconverter<Direction, bool>(
        value => {
          if (value==Direction.None)
            throw Exceptions.InvalidArgument(value, "value");
          return value==Direction.Positive;
        },
        value => value ? Direction.Positive : Direction.Negative))
    {
    }
  }
}
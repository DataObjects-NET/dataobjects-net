// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.20

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core;

namespace Xtensive.Collections
{
  /// <summary>
  /// "Order by" clause descriptor - a helper type allowing to describe "order by" clauses with ease.
  /// You must use <see cref="OrderBy"/> type to create the instances of this struct.
  /// </summary>
  /// <typeparam name="T">The type of order by clause item.</typeparam>
  public struct OrderBy<T>
  {
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private readonly IEnumerable<KeyValuePair<T, Direction>> order;

    /// <summary>
    /// Creates new <see cref="OrderBy{T}"/> with joined ", item0, item1, ..." tail.
    /// </summary>
    /// <param name="items">Items to join to the tail.</param>
    /// <returns>New <see cref="OrderBy{T}"/> with joined ", item0, item1, ..." tail.</returns>
    public OrderBy<T> Asc(params T[] items)
    {
      return new OrderBy<T>(this, Direction.Positive, items);
    }

    /// <summary>
    /// Creates new <see cref="OrderBy{T}"/> with joined ", item0 desc, item1 desc, ... desc" tail.
    /// </summary>
    /// <param name="items">Items to join to the tail.</param>
    /// <returns>New <see cref="OrderBy{T}"/> with joined ", item0 desc, item1 desc, ... desc" tail.</returns>
    public OrderBy<T> Desc(params T[] items)
    {
      return new OrderBy<T>(this, Direction.Negative, items);
    }

    /// <summary>
    /// Implicitly converts <see cref="OrderBy{T}"/> to <see cref="DirectionCollection{T}"/>.
    /// </summary>
    /// <param name="source">The "order by" clause descriptor to convert.</param>
    /// <returns>Conversion result.</returns>
    public static implicit operator DirectionCollection<T> (OrderBy<T> source)
    {
      return new DirectionCollection<T>(source.order);
    }


    // Constructors

    internal OrderBy(Direction direction, T[] items)
    {
      ArgumentValidator.EnsureArgumentNotNull(items, "items");
      ArgumentValidator.EnsureArgumentIsNotDefault(direction, "direction");
      order = items.Select(t => new KeyValuePair<T, Direction>(t, direction));
    }

    private OrderBy(OrderBy<T> current, Direction direction, T[] items)
    {
      ArgumentValidator.EnsureArgumentNotNull(items, "items");
      ArgumentValidator.EnsureArgumentIsNotDefault(direction, "direction");
      order = current.order.Union(items.Select(t => new KeyValuePair<T, Direction>(t, direction)));
    }
  }
}
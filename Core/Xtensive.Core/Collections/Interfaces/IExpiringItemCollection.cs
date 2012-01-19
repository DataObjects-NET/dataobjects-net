// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.06.13

using System;
using Xtensive.Core;

namespace Xtensive.Collections
{
  /// <summary>
  /// Describes a collection containing expiring items.
  /// </summary>
  /// <typeparam name="T">The type of collection item.</typeparam>
  public interface IExpiringItemCollection<T>: IHasGarbageCollector
  {
    /// <summary>
    /// Gets the item expiration period.
    /// </summary>
    TimeSpan ItemExpirationPeriod { get; }

    /// <summary>
    /// Occurs when item is going to expire.
    /// </summary>
    event EventHandler<ItemExpiresEventArgs<T>> ItemExpires;

    /// <summary>
    /// Occurs when item has expired.
    /// </summary>
    event EventHandler<ItemRemovedEventArgs<T>> ItemRemoved;
  }
}
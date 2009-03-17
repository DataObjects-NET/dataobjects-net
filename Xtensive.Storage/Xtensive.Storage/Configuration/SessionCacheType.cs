// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.12

using Xtensive.Core.Caching;

namespace Xtensive.Storage.Configuration
{

  /// <summary>
  /// Enumerate possible types of session caches.
  /// </summary>
  public enum SessionCacheType
  {
    /// <summary>
    /// Default cache type.
    /// Value is <see langword="0x0" />.
    /// </summary>
    Default = 0,

    /// <summary>
    /// <see cref="LruCache&lt;TKey,TItem&gt;"/> with chained <see cref="WeakCache&lt;TKey,TItem&gt;"/>.
    /// Value is <see langword="0x0" />.
    /// </summary>
    LruWeak = 0,

    /// <summary>
    /// <see cref="InfiniteCache&lt;TKey,TItem&gt;"/>.
    /// Value is <see langword="0x1" />.
    /// </summary>
    Infinite = 1
  }
}
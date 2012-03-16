// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.02.15

namespace Xtensive.Indexing
{
  /// <summary>
  /// Describes the part of result of such operation as 
  /// <see cref="IOrderedEnumerable{TKey,TItem}.Seek"/>.
  /// </summary>
  public enum SeekResultType: sbyte
  {
    /// <summary>
    /// The same as <see cref="None"/>.
    /// </summary>
    Default = None,

    /// <summary>
    /// No item is found.
    /// </summary>
    None   = 0,

    /// <summary>
    /// Exact match is found (i.e. equality condition is satisfied).
    /// </summary>
    Exact   = 1,

    /// <summary>
    /// Next nearest match is found. 
    /// Nearest - in the specified <see cref="Ray{T}.Direction"/> of the ray.
    /// </summary>
    Nearest = 2,
  }
}
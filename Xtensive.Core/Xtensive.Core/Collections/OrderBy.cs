// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.20

using System;
using Xtensive.Core;

namespace Xtensive.Collections
{
  /// <summary>
  /// A helper static type to use in conjunction with <see cref="OrderBy{T}"/>.
  /// Provides more descriptive ways to construct <see cref="OrderBy{T}"/> instance - 
  /// i.e. <see cref="Asc{T}"/> and <see cref="Desc{T}"/> methods.
  /// </summary>
  public static class OrderBy
  {
    /// <summary>
    /// Creates new <see cref="OrderBy{T}"/> containing "item0, item1, ..." list.
    /// </summary>
    /// <param name="items">Items to include into the list.</param>
    /// <returns>New <see cref="OrderBy{T}"/> containing "item0, item1, ..." list.</returns>
    public static OrderBy<T> Asc<T>(params T[] items)
    {
      return new OrderBy<T>(Direction.Positive, items);
    }

    /// <summary>
    /// Creates new <see cref="OrderBy{T}"/> containing "item0 desc, item1 desc, ... desc" list.
    /// </summary>
    /// <param name="items">Items to include into the list.</param>
    /// <returns>New <see cref="OrderBy{T}"/> containing "item0 desc, item1 desc, ... desc" list.</returns>
    public static OrderBy<T> Desc<T>(params T[] items)
    {
      return new OrderBy<T>(Direction.Negative, items);
    }
  }
}
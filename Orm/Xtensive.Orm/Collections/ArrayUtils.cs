// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Anton U. Rogozhin
// Reimplemented by: Dmitri Maximov
// Created:    2007.07.04

using System;

namespace Xtensive.Collections
{
  /// <summary>
  /// <see cref="Array"/> related utilities.
  /// </summary>
  /// <typeparam name="TItem">Type of array item.</typeparam>
  public static class ArrayUtils<TItem>
  {
    private static readonly TItem[] emptyArray = new TItem[] {};

    /// <summary>
    /// Gets empty array of items of <typeparamref name="TItem"/> type.
    /// </summary>
    public static TItem[] EmptyArray {
      get { return emptyArray; }
    }
  }
}
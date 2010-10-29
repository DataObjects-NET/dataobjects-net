// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Anton U. Rogozhin
// Reimplemented by: Dmitri Maximov
// Created:    2007.07.04

using System;
using System.Collections.Generic;
using Xtensive.Comparison;
using Xtensive.Reflection;
using Xtensive.Resources;

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

    /// <summary>
    /// Creates a new 1-dimensional array of specified <paramref name="size"/>,
    /// if <paramref name="size"/> isn't <see langword="0"/>;
    /// otherwise, returns <see cref="EmptyArray"/>.
    /// </summary>
    /// <param name="size">Size of the array to create.</param>
    /// <returns>Created array, if <paramref name="size"/> isn't <see langword="0"/>;
    /// otherwise, <see cref="EmptyArray"/>.</returns>
    public static TItem[] Create(int size)
    {
      if (size!=0)
        return new TItem[size];
      else
        return emptyArray;
    }
  }
}
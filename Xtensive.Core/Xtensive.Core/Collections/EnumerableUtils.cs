// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.04

using System.Collections;
using System.Collections.Generic;

namespace Xtensive.Core.Collections
{
  /// <summary>
  /// <see cref="IEnumerable"/> related utilities.
  /// </summary>
  public static class EnumerableUtils
  {
    /// <summary>
    /// Gets the empty sequence.
    /// </summary>
    /// <typeparam name="T">The type of enumerated items.</typeparam>
    /// <returns>Empty sequence.</returns>
    public static IEnumerable<T> GetEmpty<T>()
    {
      if (false)
        yield return default(T);
    }

    /// <summary>
    /// Gets the enumerator of empty sequence.
    /// </summary>
    /// <typeparam name="T">The type of enumerated items.</typeparam>
    /// <returns>The enumerator of empty sequence.</returns>
    public static IEnumerator<T> GetEmptyEnumerator<T>()
    {
      if (false)
        yield return default(T);
    }
  }
}
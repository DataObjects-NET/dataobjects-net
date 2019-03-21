// Copyright (C) 2003-2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kudelin
// Created:    2019.03.21

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;

namespace Xtensive.Collections
{
  /// <summary>
  /// <see cref="ICollection"/> related utilities.
  /// </summary>
  public static class CollectionUtils
  {
    /// <summary>Generates an array of integral numbers within a specified range.</summary>
    /// <param name="start">The value of the first integer in the sequence.</param>
    /// <param name="count">The number of sequential integers to generate.</param>
    /// <returns>An array that contains a range of sequential integral numbers.</returns>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="count"/> is less than 0.-or-
    /// <paramref name="start"/> + <paramref name="count"/> -1 is larger than <see cref="F:System.Int32.MaxValue"/>.</exception>
    public static int[] RangeToArray(int start, int count)
    {
      ArgumentValidator.EnsureArgumentIsGreaterThanOrEqual(count, 0, "count");
      var result = new int[count];
      var index = 0;
      foreach (var value in Enumerable.Range(start, count))
        result[index++] = value;
      return result;
    }

    /// <summary>Generates a list of integral numbers within a specified range.</summary>
    /// <param name="start">The value of the first integer in the sequence.</param>
    /// <param name="count">The number of sequential integers to generate.</param>
    /// <returns>An List&lt;Int32&gt; that contains a range of sequential integral numbers.</returns>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="count"/> is less than 0.-or-
    /// <paramref name="start"/> + <paramref name="count"/> -1 is larger than <see cref="F:System.Int32.MaxValue"/>.</exception>
    public static List<int> RangeToList(int start, int count)
    {
      ArgumentValidator.EnsureArgumentIsGreaterThanOrEqual(count, 0, "count");
      var result = new List<int>(count);
      result.AddRange(Enumerable.Range(start, count));
      return result;
    }

    /// <summary>Generates an array that contains one repeated value.</summary>
    /// <param name="element">The value to be repeated.</param>
    /// <param name="count">The number of times to repeat the value in the generated sequence.</param>
    /// <typeparam name="TResult">The type of the value to be repeated in the result sequence.</typeparam>
    /// <returns>An array that contains a repeated value.</returns>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="count"/> is less than 0.</exception>
    public static TResult[] RepeatToArray<TResult>(TResult element, int count)
    {
      ArgumentValidator.EnsureArgumentIsGreaterThanOrEqual(count, 0, "count");
      var result = new TResult[count];
      for (var i = 0; i < count; ++i)
        result[i] = element;
      return result;
    }

    /// <summary>Generates a list that contains one repeated value.</summary>
    /// <param name="element">The value to be repeated.</param>
    /// <param name="count">The number of times to repeat the value in the generated sequence.</param>
    /// <typeparam name="TResult">The type of the value to be repeated in the result sequence.</typeparam>
    /// <returns>An <see cref="T:System.Collections.Generic.List`1" /> that contains a repeated value.</returns>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="count"/> is less than 0.</exception>
    public static List<TResult> RepeatToList<TResult>(TResult element, int count)
    {
      ArgumentValidator.EnsureArgumentIsGreaterThanOrEqual(count, 0, "count");
      var result = new List<TResult>(count);
      for (var i = 0; i < count; ++i)
        result.Add(element);
      return result;
    }
  }
}
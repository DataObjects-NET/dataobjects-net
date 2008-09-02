// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.04

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Xtensive.Core.Collections
{
  /// <summary>
  /// <see cref="IEnumerable"/> related utilities.
  /// </summary>
  public static class EnumerableUtils
  {
    #region Nested type: EmptyEnumerable<T>

    internal sealed class EmptyEnumerable<T> : IEnumerable<T>
    {
      internal sealed class EmptyEnumerator : IEnumerator<T>
      {
        public void Dispose()
        {
        }

        public bool MoveNext()
        {
          return false;
        }

        public void Reset()
        {
        }

        object IEnumerator.Current
        {
          get { return default(T); }
        }

        public T Current
        {
          get { return default(T); }
        }
      }

      public static EmptyEnumerator Enumerator = new EmptyEnumerator();
      public static EmptyEnumerable<T> Instance = new EmptyEnumerable<T>();

      IEnumerator IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }

      public IEnumerator<T> GetEnumerator()
      {
        return Enumerator;
      }
    }

    #endregion

    /// <summary>
    /// Safely adds one value to sequence.
    /// </summary>
    /// <typeparam name="T">The type of enumerated items.</typeparam>
    /// <param name="source">Source sequence.</param>
    /// <param name="value">Value to add to sequence.</param>
    /// <returns>New sequence with both <paramref name="source"/> and <paramref name="value"/> items inside without duplicates.</returns>
    /// <remarks>If source sequence is null, it's equals to empty sequence. If value is null, it will not added to result sequence.</remarks>
    public static IEnumerable<T> AddOne<T>(this IEnumerable<T> source, T value)
    {
      source = source ?? GetEmpty<T>();
      if (!ReferenceEquals(value, null))
        source = source.Union(One(value));
      return source;
    }

    /// <summary>
    /// Gets the enumerable with one element.
    /// </summary>
    /// <typeparam name="T">The type of enumerated items.</typeparam>
    /// <returns>Sequence with value inside.</returns>
    public static IEnumerable<T> One<T>(T value)
    {
      yield return value;
    }

    /// <summary>
    /// Gets the empty sequence.
    /// </summary>
    /// <typeparam name="T">The type of enumerated items.</typeparam>
    /// <returns>Empty sequence.</returns>
    public static IEnumerable<T> GetEmpty<T>()
    {
      return EmptyEnumerable<T>.Instance;
    }

    /// <summary>
    /// Gets the enumerator of empty sequence.
    /// </summary>
    /// <typeparam name="T">The type of enumerated items.</typeparam>
    /// <returns>The enumerator of empty sequence.</returns>
    public static IEnumerator<T> GetEmptyEnumerator<T>()
    {
      return EmptyEnumerable<T>.Enumerator;
    }
  }
}
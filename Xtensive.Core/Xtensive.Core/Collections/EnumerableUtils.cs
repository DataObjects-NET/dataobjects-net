// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.04

using System;
using System.Collections;
using System.Collections.Generic;

namespace Xtensive.Core.Collections
{
  /// <summary>
  /// <see cref="IEnumerable"/> related utilities.
  /// </summary>
  /// <typeparam name="TItem">Type of enumerated item.</typeparam>
  public static class EnumerableUtils<TItem>
  {
    #region Nested type: EmptyEnumerable<T>

    internal sealed class EmptyEnumerable<T> : IEnumerable<T>
    {
      internal sealed class EmptyEnumerator : IEnumerator<T>
      {
        public void Reset()
        {
        }

        public bool MoveNext()
        {
          return false;
        }

        object IEnumerator.Current
        {
          get { return default(T); }
        }

        public T Current
        {
          get { return default(T); }
        }

        public void Dispose()
        {
        }
      }

      public static EmptyEnumerator EnumeratorInstance = new EmptyEnumerator();
      
      public static EmptyEnumerable<T> Instance = new EmptyEnumerable<T>();

      IEnumerator IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }

      public IEnumerator<T> GetEnumerator()
      {
        return EnumeratorInstance;
      }
    }

    #endregion

    /// <summary>
    /// Gets the enumerable with one element.
    /// </summary>
    /// <returns>Sequence with value inside.</returns>
    public static IEnumerable<TItem> One(TItem value)
    {
      yield return value;
    }

    /// <summary>
    /// Gets the empty sequence.
    /// </summary>
    /// <returns>Empty sequence.</returns>
    public static IEnumerable<TItem> Empty {
      get { return EmptyEnumerable<TItem>.Instance; }
    }

    /// <summary>
    /// Unfolds the whole sequence from its <paramref name="first"/> item.
    /// If <paramref name="first"/> is <see langword="null" />,
    /// an empty sequence is returned.
    /// </summary>
    /// <param name="first">The first item.</param>
    /// <param name="nextItemGenerator">The delegate returning the next item by the current one.
    /// The enumeration continues until it returns <see langword="null" />.</param>
    /// <returns>Unfolded sequence of items 
    /// starting from the <paramref name="first"/> one.</returns>
    public static IEnumerable<TItem> Unfold(TItem first, Func<TItem, TItem> nextItemGenerator)
    {
      ArgumentValidator.EnsureArgumentNotNull(nextItemGenerator, "nextItemGenerator");
      var current = first;
      while (current!=null) {
        yield return current;
        current = nextItemGenerator.Invoke(current);
      }
    }

    /// <summary>
    /// Gets the enumerator of empty sequence.
    /// </summary>
    /// <returns>The enumerator of empty sequence.</returns>
    public static IEnumerator<TItem> EmptyEnumerator {
      get { return EmptyEnumerable<TItem>.EnumeratorInstance; }
    }
  }
}
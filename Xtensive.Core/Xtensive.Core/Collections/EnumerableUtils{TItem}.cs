// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.04

using System;
using System.Collections;
using System.Collections.Generic;

namespace Xtensive.Collections
{
  /// <summary>
  /// <see cref="IEnumerable"/> related utilities.
  /// </summary>
  /// <typeparam name="TItem">The type of enumerated item.</typeparam>
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
    /// Gets the empty sequence.
    /// </summary>
    /// <returns>Empty sequence.</returns>
    public static IEnumerable<TItem> Empty {
      get { return EmptyEnumerable<TItem>.Instance; }
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
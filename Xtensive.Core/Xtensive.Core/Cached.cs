// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.24

using System;
using System.Diagnostics;
using System.Threading;
using Xtensive.Core.Comparison;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Resources;

namespace Xtensive.Core
{
  /// <summary>
  /// A structure caching a single value of type <typeparamref name="T"/>.
  /// </summary>
  /// <typeparam name="T">Type of the value to cache.</typeparam>
  [Serializable]
  [DebuggerDisplay("{value}")]
  public struct Cached<T> : 
    IEquatable<Cached<T>>,
    IComparable<Cached<T>>
  {
    private T cachedValue;
    private bool isCached;

    #region GetValue methods (regular)

    /// <summary>
    /// Gets the cached value or generates it using specified <paramref name="generator"/> and caches.
    /// </summary>
    /// <param name="generator">The value generator.</param>
    /// <returns>Cached value.</returns>
    public T GetValue(Func<T> generator)
    {
      if (!isCached) {
        var value = generator.Invoke();
        isCached = true;
        cachedValue = value;
      }
      return cachedValue;
    }

    /// <summary>
    /// Gets the cached value or generates it using specified <paramref name="generator"/> and caches.
    /// </summary>
    /// <typeparam name="T1">The type of the <paramref name="argument"/> to pass to the <paramref name="generator"/>.</typeparam>
    /// <param name="generator">The value generator.</param>
    /// <param name="argument">The argument to pass to the <paramref name="generator"/>.</param>
    /// <returns>Cached value.</returns>
    public T GetValue<T1>(Func<T1, T> generator, T1 argument)
    {
      if (!isCached) {
        var value = generator.Invoke(argument);
        isCached = true;
        cachedValue = value;
      }
      return cachedValue;
    }

    /// <summary>
    /// Gets the cached value or generates it using specified <paramref name="generator"/> and caches.
    /// </summary>
    /// <typeparam name="T1">The type of the <paramref name="argument1"/> to pass to the <paramref name="generator"/>.</typeparam>
    /// <typeparam name="T2">The type of the <paramref name="argument2"/> to pass to the <paramref name="generator"/>.</typeparam>
    /// <param name="generator">The value generator.</param>
    /// <param name="argument1">The first argument to pass to the <paramref name="generator"/>.</param>
    /// <param name="argument2">The second argument to pass to the <paramref name="generator"/>.</param>
    /// <returns>Cached value.</returns>
    public T GetValue<T1, T2>(Func<T1, T2, T> generator, T1 argument1, T2 argument2)
    {
      if (!isCached) {
        var value = generator.Invoke(argument1, argument2);
        isCached = true;
        cachedValue = value;
      }
      return cachedValue;
    }

    #endregion

    #region GetValue methods (thread-safe)

    /// <summary>
    /// Gets the cached value or generates it using specified <paramref name="generator"/> and caches.
    /// </summary>
    /// <param name="syncRoot">The object to synchronize on (<see cref="Monitor"/> class is used).</param>
    /// <param name="generator">The value generator.</param>
    /// <returns>Cached value.</returns>
    public T GetValue(object syncRoot, Func<T> generator)
    {
      if (!isCached) lock (syncRoot) if (!isCached) {
        var value = generator.Invoke();
        isCached = true;
        cachedValue = value;
      }
      return cachedValue;
    }

    /// <summary>
    /// Gets the cached value or generates it using specified <paramref name="generator"/> and caches.
    /// </summary>
    /// <typeparam name="T1">The type of the <paramref name="argument"/> to pass to the <paramref name="generator"/>.</typeparam>
    /// <param name="syncRoot">The object to synchronize on (<see cref="Monitor"/> class is used).</param>
    /// <param name="generator">The value generator.</param>
    /// <param name="argument">The argument to pass to the <paramref name="generator"/>.</param>
    /// <returns>Cached value.</returns>
    public T GetValue<T1>(object syncRoot, Func<T1, T> generator, T1 argument)
    {
      if (!isCached) lock (syncRoot) if (!isCached) {
        var value = generator.Invoke(argument);
        isCached = true;
        cachedValue = value;
      }
      return cachedValue;
    }

    /// <summary>
    /// Gets the cached value or generates it using specified <paramref name="generator"/> and caches.
    /// </summary>
    /// <typeparam name="T1">The type of the <paramref name="argument1"/> to pass to the <paramref name="generator"/>.</typeparam>
    /// <typeparam name="T2">The type of the <paramref name="argument2"/> to pass to the <paramref name="generator"/>.</typeparam>
    /// <param name="syncRoot">The object to synchronize on (<see cref="Monitor"/> class is used).</param>
    /// <param name="generator">The value generator.</param>
    /// <param name="argument1">The first argument to pass to the <paramref name="generator"/>.</param>
    /// <param name="argument2">The second argument to pass to the <paramref name="generator"/>.</param>
    /// <returns>Cached value.</returns>
    public T GetValue<T1, T2>(object syncRoot, Func<T1, T2, T> generator, T1 argument1, T2 argument2)
    {
      if (!isCached) lock (syncRoot) if (!isCached) {
        var value = generator.Invoke(argument1, argument2);
        isCached = true;
        cachedValue = value;
      }
      return cachedValue;
    }

    #endregion

    #region IComparable<...>, IEquatable<...> methods

    /// <inheritdoc/>
    public bool Equals(Cached<T> other)
    {
      if (isCached!=other.isCached)
        return false;
      return AdvancedComparerStruct<T>.System.Equals(cachedValue, other.cachedValue);
    }

    /// <inheritdoc/>
    public int CompareTo(Cached<T> other)
    {
      int result = (isCached ? 1 : 0) - (other.isCached ? 1 : 0);
      if (result!=0)
        return result;
      return AdvancedComparerStruct<T>.System.Compare(cachedValue, other.cachedValue);
    }

    #endregion

    #region Equals, GetHashCode, ==, !=

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (obj.GetType()!=typeof (Cached<T>))
        return false;
      return Equals((Cached<T>) obj);
    }

    public override int GetHashCode()
    {
      unchecked {
        return 
          (isCached.GetHashCode() * 397) ^ 
          (cachedValue!=null ? cachedValue.GetHashCode() : 0);
      }
    }

    public static bool operator ==(Cached<T> left, Cached<T> right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(Cached<T> left, Cached<T> right)
    {
      return !left.Equals(right);
    }

    #endregion

    #region Private \ internal methods

    /// <exception cref="InvalidOperationException">No value is cached.</exception>
    private void EnsureHasValue()
    {
      if (!isCached)
        throw new InvalidOperationException(Strings.ExValueIsNotAvailable);
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return String.Format(Strings.CachedFormat, cachedValue);
    }
  }
}
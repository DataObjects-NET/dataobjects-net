// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.24

using System;
using System.Diagnostics;
using System.Threading;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Threading;
using Xtensive.Resources;

namespace Xtensive.Threading
{
  /// <summary>
  /// A structure caching a single value of type <typeparamref name="T"/>.
  /// </summary>
  /// <typeparam name="T">Type of the value to cache.</typeparam>
  [Serializable]
  [DebuggerDisplay("{value}")]
  public struct ThreadSafeCached<T> : 
    IEquatable<ThreadSafeCached<T>>,
    IComparable<ThreadSafeCached<T>>,
    ISynchronizable
  {
    private T cachedValue;
    private volatile bool isCached;
    private object syncRoot;

    /// <inheritdoc/>
    public object SyncRoot {
      [DebuggerStepThrough]
      get { return syncRoot; }
    }

    /// <inheritdoc/>
    public bool IsSynchronized
    {
      [DebuggerStepThrough]
      get { return true; }
    }

    #region GetValue methods

    /// <summary>
    /// Gets the cached value or generates it using specified <paramref name="generator"/> and caches.
    /// </summary>
    /// <param name="generator">The value generator.</param>
    /// <returns>Cached value.</returns>
    public T GetValue(Func<T> generator)
    {
      if (!isCached) lock (syncRoot) if (!isCached) {
        cachedValue = generator.Invoke();
        Thread.MemoryBarrier(); // Ensures cachedValue is fully written
        isCached = true;
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
      if (!isCached) lock (syncRoot) if (!isCached) {
        cachedValue = generator.Invoke(argument);
        Thread.MemoryBarrier(); // Ensures cachedValue is fully written
        isCached = true;
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
      if (!isCached) lock (syncRoot) if (!isCached) {
        cachedValue = generator.Invoke(argument1, argument2);
        Thread.MemoryBarrier(); // Ensures cachedValue is fully written
        isCached = true;
      }
      return cachedValue;
    }

    #endregion

    #region IComparable<...>, IEquatable<...> methods

    /// <inheritdoc/>
    public bool Equals(ThreadSafeCached<T> other)
    {
      if (isCached!=other.isCached)
        return false;
      return AdvancedComparerStruct<T>.System.Equals(cachedValue, other.cachedValue);
    }

    /// <inheritdoc/>
    public int CompareTo(ThreadSafeCached<T> other)
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
      if (obj.GetType()!=typeof (ThreadSafeCached<T>))
        return false;
      return Equals((ThreadSafeCached<T>) obj);
    }

    public override int GetHashCode()
    {
      unchecked {
        return 
          (isCached.GetHashCode() * 397) ^ 
            (cachedValue!=null ? cachedValue.GetHashCode() : 0);
      }
    }

    public static bool operator ==(ThreadSafeCached<T> left, ThreadSafeCached<T> right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(ThreadSafeCached<T> left, ThreadSafeCached<T> right)
    {
      return !left.Equals(right);
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(Strings.ThreadSafeCachedFormat, cachedValue);
    }

    /// <summary>
    /// Initializes the cache. 
    /// This method should be invoked just once - before
    /// the first operation on this structure.
    /// </summary>
    /// <param name="syncRoot"><see cref="SyncRoot"/> property value.</param>
    public void Initialize(object syncRoot)
    {
      if (this.syncRoot!=null)
        throw Exceptions.AlreadyInitialized(null);
      this.syncRoot = syncRoot;
    }

   
    // Static constructor replacement
    
    /// <summary>
    /// Creates and initializes a new <see cref="ThreadSafeCached{T}"/>.
    /// </summary>
    /// <param name="syncRoot"><see cref="SyncRoot"/> property value.</param>
    /// <returns>New initialized <see cref="ThreadSafeCached{T}"/>.</returns>
    public static ThreadSafeCached<T> Create(object syncRoot)
    {
      var result = new ThreadSafeCached<T>();
      result.Initialize(syncRoot);
      return result;
    }
  }
}
// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.06

using System;
using System.Diagnostics;
using System.Threading;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Threading;

namespace Xtensive.Threading
{
  /// <summary>
  /// Thread-safe list. Any operation on it is atomic.
  /// Note: it recreates its internal array (makes it twice larger) when it should grow up.
  /// </summary>
  /// <typeparam name="TItem">Value type.</typeparam>
  [Serializable]
  public struct ThreadSafeList<TItem> :
    ISynchronizable
  {
    private const int InitialSize = 16;
    private readonly static TItem defaultItem = default(TItem);    
    private volatile TItem[] implementation;

    /// <inheritdoc/>
    public object SyncRoot { get; private set; }

    /// <inheritdoc/>
    public bool IsSynchronized
    {
      [DebuggerStepThrough]
      get { return true; }
    }

    #region GetValue methods with generator

    /// <summary>
    /// Gets the value or generates it using specified <paramref name="generator"/> and 
    /// adds it to the list.
    /// </summary>
    /// <param name="index">The index of the value to get.</param>
    /// <param name="generator">The value generator.</param>
    /// <returns>Found or generated value.</returns>
    public TItem GetValue(int index, Func<int, TItem> generator)
    {
      TItem item = GetValue(index);
      if (!ReferenceEquals(item, defaultItem))
        return item;
      else lock (SyncRoot) {
        item = GetValue(index);
        if (!ReferenceEquals(item, defaultItem))
          return item;
        item = generator.Invoke(index);
        SetValue(index, item);
        return item;
      }
    }

    /// <summary>
    /// Gets the value or generates it using specified <paramref name="generator"/> and 
    /// adds it to the list.
    /// </summary>
    /// <typeparam name="T">The type of the <paramref name="argument"/> to pass to the <paramref name="generator"/>.</typeparam>
    /// <param name="index">The index of the value to get.</param>
    /// <param name="generator">The value generator.</param>
    /// <param name="argument">The argument to pass to the <paramref name="generator"/>.</param>
    /// <returns>Found or generated value.</returns>
    public TItem GetValue<T>(int index, Func<int, T, TItem> generator, T argument)
    {
      TItem item = GetValue(index);
      if (!ReferenceEquals(item, defaultItem))
        return item;
      else lock (SyncRoot) {
        item = GetValue(index);
        if (!ReferenceEquals(item, defaultItem))
          return item;
        item = generator.Invoke(index, argument);
        SetValue(index, item);
        return item;
      }
    }

    /// <summary>
    /// Gets the value or generates it using specified <paramref name="generator"/> and 
    /// adds it to the list.
    /// </summary>
    /// <typeparam name="T1">The type of the <paramref name="argument1"/> to pass to the <paramref name="generator"/>.</typeparam>
    /// <typeparam name="T2">The type of the <paramref name="argument2"/> to pass to the <paramref name="generator"/>.</typeparam>
    /// <param name="index">The index of the value to get.</param>
    /// <param name="generator">The value generator.</param>
    /// <param name="argument1">The first argument to pass to the <paramref name="generator"/>.</param>
    /// <param name="argument2">The second argument to pass to the <paramref name="generator"/>.</param>
    /// <returns>Found or generated value.</returns>
    public TItem GetValue<T1, T2>(int index, Func<int, T1, T2, TItem> generator, T1 argument1, T2 argument2)
    {
      TItem item = GetValue(index);
      if (!ReferenceEquals(item, defaultItem))
        return item;
      else lock (SyncRoot) {
        item = GetValue(index);
        if (!ReferenceEquals(item, defaultItem))
          return item;
        item = generator.Invoke(index, argument1, argument2);
        SetValue(index, item);
        return item;
      }
    }

    #endregion

    #region Base methods: GetValue, SetValue, Clear

    /// <summary>
    /// Gets the value by its index.
    /// </summary>
    /// <param name="index">The index to get value for.</param>
    /// <returns>Found value, or <see langword="default(TItem)"/>.</returns>
    public TItem GetValue(int index)
    {
      if (index<0 || index>=implementation.Length)
        return defaultItem;
      else
        return implementation[index];
    }

    /// <summary>
    /// Sets the value associated with specified index.
    /// </summary>
    /// <param name="index">The index to set value for.</param>
    /// <param name="item">The value to set.</param>
    public void SetValue(int index, TItem item)
    {
      lock (implementation) {
        if (index<0)
          throw new ArgumentOutOfRangeException("index");
        int length = implementation.Length;
        if (index<length) {
          Thread.MemoryBarrier(); // Ensures item is fully written
          implementation[index] = item;
          return;
        }
        while (index>=length) checked {
          length *= 2;
        }
        TItem[] tmp = new TItem[length];
        implementation.Copy(tmp, 0);
        tmp[index] = item;
        Thread.MemoryBarrier(); // Ensures item and tmp are fully written
        implementation = tmp;
      }
    }

    /// <summary>
    /// Clears the list.
    /// </summary>
    public void Clear()
    {
      lock (implementation) {
        var tmp = new TItem[InitialSize];
        Thread.MemoryBarrier(); // Ensures tmp is fully written
        implementation = tmp;
      }
    }

    #endregion

    /// <summary>
    /// Initializes the list. 
    /// This method should be invoked just once - before
    /// the first operation on this list.
    /// </summary>
    /// <param name="syncRoot"><see cref="SyncRoot"/> property value.</param>
    public void Initialize(object syncRoot)
    {
      if (implementation!=null)
        throw Exceptions.AlreadyInitialized(null);
      this.SyncRoot = syncRoot;
      var tmp = new TItem[InitialSize];
      Thread.MemoryBarrier(); // Ensures tmp is fully written
      implementation = tmp;
    }


    // Static constructor replacement
    
    /// <summary>
    /// Creates and initializes a new <see cref="ThreadSafeList{TItem}"/>.
    /// </summary>
    /// <param name="syncRoot"><see cref="SyncRoot"/> property value.</param>
    /// <returns>New initialized <see cref="ThreadSafeList{TItem}"/>.</returns>
    public static ThreadSafeList<TItem> Create(object syncRoot)
    {
      var result = new ThreadSafeList<TItem>();
      result.Initialize(syncRoot);
      return result;
    }
  }
}
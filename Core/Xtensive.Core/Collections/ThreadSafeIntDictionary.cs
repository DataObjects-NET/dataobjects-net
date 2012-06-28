// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.07.27

using System;
using System.Diagnostics;
using System.Threading;
using Xtensive.Core;


namespace Xtensive.Collections
{
  /// <summary>
  /// Thread-safe version of <see cref="IntDictionary{TValue}"/>. Any operation on it is atomic.
  /// Note: it recreates its internal dictionary on any data modifying
  /// operation on it.
  /// </summary>
  /// <typeparam name="TValue">Value type.</typeparam>
  [Serializable]
  public struct ThreadSafeIntDictionary<TValue>
  {
    private IntDictionary<TValue> implementation;
    private object syncRoot;
    
    /// <inheritdoc/>
    public object SyncRoot {
      [DebuggerStepThrough]
      get { return syncRoot; }
    }

    /// <summary>
    /// Gets the value or generates it using specified <paramref name="generator"/> and 
    /// adds it to the dictionary.
    /// </summary>
    /// <param name="key">The key to get the value for.</param>
    /// <param name="generator">The value generator.</param>
    /// <returns>Found or generated value.</returns>
    public TValue GetValue(int key, Func<int, TValue> generator)
    {
      TValue value;
      if (TryGetValue(key, out value))
        return value;
      lock (syncRoot) {
        if (TryGetValue(key, out value))        
          return value;
        var newItem = generator.Invoke(key);
        SetValue(key, newItem);
        return newItem;
      }
    }

    /// <summary>
    /// Gets the value or generates it using specified <paramref name="generator"/> and 
    /// adds it to the dictionary.
    /// </summary>
    /// <typeparam name="T">The type of the <paramref name="argument"/> to pass to 
    /// the <paramref name="generator"/>.</typeparam>
    /// <param name="key">The key to get the value for.</param>
    /// <param name="generator">The value generator.</param>
    /// <param name="argument">The argument to pass to the <paramref name="generator"/>.</param>
    /// <returns>Found or generated value.</returns>
    public TValue GetValue<T>(int key, Func<int, T, TValue> generator, T argument)
    {
      TValue value;
      if (TryGetValue(key, out value))
        return value;
      lock (syncRoot) {
        if (TryGetValue(key, out value))        
          return value;
        TValue newItem = generator.Invoke(key, argument);
        SetValue(key, newItem);
        return newItem;
      }
    }
    
    /// <summary>
    /// Gets the value by its key.
    /// </summary>
    /// <param name="key">The key to get the value for.</param>
    /// <param name="value">Found value, or default value, if value is not found.</param>
    /// <returns>Whether or not value was found.</returns>    
    public bool TryGetValue(int key, out TValue value)
    {
      return implementation.TryGetValue(key, out value);
    }

    /// <summary>
    /// Sets the value associated with specified key.
    /// </summary>
    /// <param name="key">The key to set value for.</param>
    /// <param name="value">The value to set.</param>
    public void SetValue(int key, TValue value)
    {
      lock (syncRoot) {
        implementation[key] = value;
      }
    }

    /// <summary>
    /// Clears the dictionary.
    /// </summary>
    public void Clear()
    {
      lock (syncRoot) {
        implementation.Clear();
        Thread.MemoryBarrier();
      }
    }

    /// <summary>
    /// Initializes the dictionary.
    /// This method should be invoked just once - before
    /// the first operation on this dictionary.
    /// </summary>
    /// <param name="syncRoot"><see cref="SyncRoot"/> property value.</param>
    /// <param name="capacity">The initial capacity.</param>
    /// <exception cref="NotSupportedException">The dictionary is already initialized.</exception>
    public void Initialize(object syncRoot, int capacity)
    {
      if (implementation!=null)
        throw Exceptions.AlreadyInitialized(null);
      this.syncRoot = syncRoot;
      var tmp = new IntDictionary<TValue>(capacity);
      Thread.MemoryBarrier(); // Ensures tmp is fully written
      implementation = tmp;
    }

    /// <summary>
    /// Initializes the dictionary.
    /// This method should be invoked just once - before
    /// the first operation on this dictionary.
    /// </summary>
    /// <param name="syncRoot"><see cref="SyncRoot"/> property value.</param>
    /// <exception cref="NotSupportedException">The dictionary is already initialized.</exception>
    public void Initialize(object syncRoot)
    {
      if (implementation!=null)
        throw Exceptions.AlreadyInitialized(null);
      this.syncRoot = syncRoot;
      var tmp = new IntDictionary<TValue>();
      Thread.MemoryBarrier(); // Ensures tmp is fully written
      implementation = tmp;
    }


    // Static constructor replacement
    
    /// <summary>
    /// Creates and initializes a new <see cref="ThreadSafeIntDictionary{TValue}"/>.
    /// </summary>
    /// <param name="syncRoot"><see cref="SyncRoot"/> property value.</param>
    /// <param name="capacity">The initial capacity.</param>
    /// <returns>New initialized <see cref="ThreadSafeIntDictionary{TValue}"/>.</returns>
    public static ThreadSafeIntDictionary<TValue> Create(object syncRoot, int capacity)
    {
      var result = new ThreadSafeIntDictionary<TValue>();
      result.Initialize(syncRoot, capacity);
      return result;
    }

    /// <summary>
    /// Creates and initializes a new <see cref="ThreadSafeIntDictionary{TValue}"/>.
    /// </summary>
    /// <param name="syncRoot"><see cref="SyncRoot"/> property value.</param>
    /// <returns>New initialized <see cref="ThreadSafeIntDictionary{TValue}"/>.</returns>
    public static ThreadSafeIntDictionary<TValue> Create(object syncRoot)
    {
      var result = new ThreadSafeIntDictionary<TValue>();
      result.Initialize(syncRoot);
      return result;
    }
  }
}
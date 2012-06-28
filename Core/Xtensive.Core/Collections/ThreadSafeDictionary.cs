// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.05

using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using Xtensive.Core;


namespace Xtensive.Collections
{
  /// <summary>
  /// Thread-safe dictionary. Any operation on it is atomic.
  /// Note: it recreates its internal dictionary on any data modifying
  /// operation on it.
  /// </summary>
  /// <typeparam name="TKey">Key type.</typeparam>
  /// <typeparam name="TItem">Value type.</typeparam>
  [Serializable]
  public struct ThreadSafeDictionary<TKey, TItem> :
    ISynchronizable
  {
    private readonly static TItem defaultItem = default(TItem);
    private Hashtable implementation;
    private object syncRoot;
    private static object nullObject = new object();


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

    #region GetValue methods with generator

    /// <summary>
    /// Gets the value or generates it using specified <paramref name="generator"/> and 
    /// adds it to the dictionary.
    /// </summary>
    /// <param name="key">The key to get the value for.</param>
    /// <param name="generator">The value generator.</param>
    /// <returns>Found or generated value.</returns>
    public TItem GetValue(TKey key, Func<TKey, TItem> generator)
    {
      TItem value;
      if (TryGetValue(key, out value))
        return value;
      else lock (syncRoot) {
        if (TryGetValue(key, out value))        
          return value;
        TItem newItem = generator.Invoke(key);
        SetValue(key, newItem);
        return newItem;
      }
    }

    /// <summary>
    /// Gets the value or generates it using specified <paramref name="generator"/> and 
    /// adds it to the dictionary.
    /// </summary>
    /// <typeparam name="T">The type of the <paramref name="argument"/> to pass to the <paramref name="generator"/>.</typeparam>
    /// <param name="key">The key to get the value for.</param>
    /// <param name="generator">The value generator.</param>
    /// <param name="argument">The argument to pass to the <paramref name="generator"/>.</param>
    /// <returns>Found or generated value.</returns>
    public TItem GetValue<T>(TKey key, Func<TKey, T, TItem> generator, T argument)
    {
      TItem value;
      if (TryGetValue(key, out value))
        return value;
      else lock (syncRoot) {
        if (TryGetValue(key, out value))        
          return value;
        TItem newItem = generator.Invoke(key, argument);
        SetValue(key, newItem);
        return newItem;
      }
    }

    /// <summary>
    /// Gets the value or generates it using specified <paramref name="generator"/> and 
    /// adds it to the dictionary.
    /// </summary>
    /// <typeparam name="T1">The type of the <paramref name="argument1"/> to pass to the <paramref name="generator"/>.</typeparam>
    /// <typeparam name="T2">The type of the <paramref name="argument2"/> to pass to the <paramref name="generator"/>.</typeparam>
    /// <param name="key">The key to get the value for.</param>
    /// <param name="generator">The value generator.</param>
    /// <param name="argument1">The first argument to pass to the <paramref name="generator"/>.</param>
    /// <param name="argument2">The second argument to pass to the <paramref name="generator"/>.</param>
    /// <returns>Found or generated value.</returns>
    public TItem GetValue<T1, T2>(TKey key, Func<TKey, T1, T2, TItem> generator, T1 argument1, T2 argument2)
    {
      TItem value;
      if (TryGetValue(key, out value))
        return value;
      else lock (syncRoot) {
        if (TryGetValue(key, out value))        
          return value;
        value = generator.Invoke(key, argument1, argument2);
        SetValue(key, value);
        return value;
      }
    }

    #endregion

    #region Base methods: GetValue, SetValue, Clear

    /// <summary>
    /// Gets the value by its key.
    /// </summary>
    /// <param name="key">The key to get the value for.</param>
    /// <param name="value">Found value, or default value, if value is not found.</param>
    /// <returns>Whether or not value was found.</returns>    
    public bool TryGetValue(TKey key, out TItem value)
    {
      object storedValue = implementation[key];
      if (storedValue==null) {
        value = default(TItem);
        return false;
      }
      else {
        value = 
          storedValue == nullObject ? default(TItem) : (TItem) storedValue;
        return true;
      }      
    }

    /// <summary>
    /// Sets the value associated with specified key.
    /// </summary>
    /// <param name="key">The key to set value for.</param>
    /// <param name="item">The value to set.</param>
    public void SetValue(TKey key, TItem item)
    {
      lock (syncRoot) {
        Thread.MemoryBarrier(); // Ensures item is fully written
        implementation[key] = (object)item ?? nullObject;
      }
    }

    /// <summary>
    /// Clears the dictionary.
    /// </summary>
    public void Clear()
    {
      lock (syncRoot) {
        implementation.Clear();
        Thread.MemoryBarrier(); // Not sure, if this is necessary, but...
      }
    }

    #endregion

    /// <summary>
    /// Initializes the dictionary. 
    /// This method should be invoked just once - before
    /// the first operation on this dictionary.
    /// </summary>
    /// <param name="syncRoot"><see cref="SyncRoot"/> property value.</param>
    public void Initialize(object syncRoot)
    {
      if (implementation!=null)
        throw Exceptions.AlreadyInitialized(null);
      this.syncRoot = syncRoot;
      var tmp = new Hashtable();
      Thread.MemoryBarrier(); // Ensures tmp is fully written
      implementation = tmp;
    }


    // Static constructor replacement
    
    /// <summary>
    /// Creates and initializes a new <see cref="ThreadSafeDictionary{TKey,TItem}"/>.
    /// </summary>
    /// <param name="syncRoot"><see cref="SyncRoot"/> property value.</param>
    /// <returns>New initialized <see cref="ThreadSafeDictionary{TKey,TItem}"/>.</returns>
    public static ThreadSafeDictionary<TKey, TItem> Create(object syncRoot)
    {
      var result = new ThreadSafeDictionary<TKey, TItem>();
      result.Initialize(syncRoot);
      return result;
    }
  }
}
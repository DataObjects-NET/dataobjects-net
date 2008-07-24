// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.05

using System;
using System.Collections.Generic;
using System.Threading;

namespace Xtensive.Core.Collections
{
  /// <summary>
  /// Thread-safe dictionary. Any operation on it is atomic.
  /// Note: it recreates its internal dictionary on any data modifying
  /// operation on it.
  /// </summary>
  /// <typeparam name="TKey">Key type.</typeparam>
  /// <typeparam name="TItem">Value type.</typeparam>
  [Serializable]
  public struct ThreadSafeDictionary<TKey, TItem>
  {
    private readonly static TItem defaultItem = default(TItem);
    private Dictionary<TKey, TItem> implementation;

    #region GetValue methods with generator

    /// <summary>
    /// Gets the value or generates it using specified <paramref name="generator"/> and 
    /// adds it to the dictionary.
    /// </summary>
    /// <param name="syncRoot">The object to synchronize on (<see cref="Monitor"/> class is used).</param>
    /// <param name="key">The key to get the value for.</param>
    /// <param name="generator">The value generator.</param>
    /// <returns>Found or generated value.</returns>
    public TItem GetValue(object syncRoot, TKey key, Func<TKey, TItem> generator)
    {
      TItem item;
      if (implementation.TryGetValue(key, out item))
        return item;
      else lock (syncRoot) {
        if (implementation.TryGetValue(key, out item))
          return item;
        item = generator.Invoke(key);
        SetValue(key, item);
        return item;
      }
    }

    /// <summary>
    /// Gets the value or generates it using specified <paramref name="generator"/> and 
    /// adds it to the dictionary.
    /// </summary>
    /// <typeparam name="T">The type of the <paramref name="argument"/> to pass to the <paramref name="generator"/>.</typeparam>
    /// <param name="syncRoot">The object to synchronize on (<see cref="Monitor"/> class is used).</param>
    /// <param name="key">The key to get the value for.</param>
    /// <param name="generator">The value generator.</param>
    /// <param name="argument">The argument to pass to the <paramref name="generator"/>.</param>
    /// <returns>Found or generated value.</returns>
    public TItem GetValue<T>(object syncRoot, TKey key, Func<TKey, T, TItem> generator, T argument)
    {
      TItem item;
      if (implementation.TryGetValue(key, out item))
        return item;
      else lock (syncRoot) {
        if (implementation.TryGetValue(key, out item))
          return item;
        item = generator.Invoke(key, argument);
        SetValue(key, item);
        return item;
      }
    }

    /// <summary>
    /// Gets the value or generates it using specified <paramref name="generator"/> and 
    /// adds it to the dictionary.
    /// </summary>
    /// <typeparam name="T1">The type of the <paramref name="argument1"/> to pass to the <paramref name="generator"/>.</typeparam>
    /// <typeparam name="T2">The type of the <paramref name="argument2"/> to pass to the <paramref name="generator"/>.</typeparam>
    /// <param name="syncRoot">The object to synchronize on (<see cref="Monitor"/> class is used).</param>
    /// <param name="key">The key to get the value for.</param>
    /// <param name="generator">The value generator.</param>
    /// <param name="argument1">The first argument to pass to the <paramref name="generator"/>.</param>
    /// <param name="argument2">The second argument to pass to the <paramref name="generator"/>.</param>
    /// <returns>Found or generated value.</returns>
    public TItem GetValue<T1, T2>(object syncRoot, TKey key, Func<TKey, T1, T2, TItem> generator, T1 argument1, T2 argument2)
    {
      TItem item;
      if (implementation.TryGetValue(key, out item))
        return item;
      else lock (syncRoot) {
        if (implementation.TryGetValue(key, out item))
          return item;
        item = generator.Invoke(key, argument1, argument2);
        SetValue(key, item);
        return item;
      }
    }

    #endregion

    #region Base methods: GetValue, SetValue, Clear

    /// <summary>
    /// Gets the value by its key.
    /// </summary>
    /// <param name="key">The key to get the value for.</param>
    /// <returns>Found value, or <see langword="default(TItem)"/>.</returns>
    public TItem GetValue(TKey key)
    {
      TItem item;
      if (implementation.TryGetValue(key, out item))
        return item;
      else
        return defaultItem;
    }

    /// <summary>
    /// Sets the value associated with specified key.
    /// </summary>
    /// <param name="key">The key to set value for.</param>
    /// <param name="item">The value to set.</param>
    public void  SetValue(TKey key, TItem item)
    {
      lock (implementation) {
        Dictionary<TKey, TItem> newImplementation = new Dictionary<TKey, TItem>(implementation);
        newImplementation[key] = item;
        implementation = newImplementation;
      }
    }

    /// <summary>
    /// Clears the dictionary.
    /// </summary>
    public void Clear()
    {
      lock (implementation) {
        implementation = new Dictionary<TKey, TItem>();
      }
    }

    #endregion

    /// <summary>
    /// Initializes the dictionary. 
    /// This method should be invoked just once - before
    /// the first operation on this dictionary.
    /// </summary>
    public void Initialize()
    {
      if (implementation!=null)
        throw Exceptions.AlreadyInitialized(null);
      implementation = new Dictionary<TKey, TItem>();
    }


    // Static constructor replacement
    
    /// <summary>
    /// Creates and initializes a new <see cref="ThreadSafeDictionary{TKey,TItem}"/>.
    /// </summary>
    /// <returns>New initialized <see cref="ThreadSafeDictionary{TKey,TItem}"/>.</returns>
    public static ThreadSafeDictionary<TKey, TItem> Create()
    {
      ThreadSafeDictionary<TKey, TItem> result = new ThreadSafeDictionary<TKey, TItem>();
      result.Initialize();
      return result;
    }
  }
}
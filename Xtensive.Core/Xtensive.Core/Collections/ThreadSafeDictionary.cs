// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.05

using System;
using System.Collections.Generic;

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

    /// <summary>
    /// Gets the value by its key.
    /// </summary>
    /// <param name="key">The key to get value for.</param>
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
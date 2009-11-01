// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.06

using System;

namespace Xtensive.Core.Collections
{
  /// <summary>
  /// Thread-safe list. Any operation on it is atomic.
  /// Note: it recreates its internal array (makes it twice larger) when it should grow up.
  /// </summary>
  /// <typeparam name="TItem">Value type.</typeparam>
  [Serializable]
  public struct ThreadSafeList<TItem>
  {
    private const int InitialSize = 16;
    private readonly static TItem defaultItem = default(TItem);
    private TItem[] implementation;

    /// <summary>
    /// Gets the value by its index.
    /// </summary>
    /// <param name="index">The index to get value for.</param>
    /// <returns>Found value, or <see langword="default(TItem)"/>.</returns>
    public TItem GetValue(int index)
    {
      try {
        return implementation[index];
      }
      catch (IndexOutOfRangeException) {
        return defaultItem;
      }
    }

    /// <summary>
    /// Sets the value associated with specified index.
    /// </summary>
    /// <param name="index">The index to set value for.</param>
    /// <param name="item">The value to set.</param>
    public void  SetValue(int index, TItem item)
    {
      lock (implementation) {
        if (index<0)
          throw new ArgumentOutOfRangeException("index");
        int length = implementation.Length;
        if (index<length) {
          implementation[index] = item;
          return;
        }
        while (index>=length) checked {
          length *= 2;
        }
        TItem[] newImplementation = new TItem[length];
        implementation.Copy(newImplementation, 0);
        newImplementation[index] = item;
        implementation = newImplementation;
      }
    }

    /// <summary>
    /// Clears the list.
    /// </summary>
    public void Clear()
    {
      lock (implementation) {
        implementation = new TItem[InitialSize];
      }
    }

    /// <summary>
    /// Initializes the list. 
    /// This method should be invoked just once - before
    /// the first operation on this list.
    /// </summary>
    public void Initialize()
    {
      if (implementation!=null)
        throw Exceptions.AlreadyInitialized(null);
      implementation = new TItem[InitialSize];
    }


    // Static constructor replacement
    
    /// <summary>
    /// Creates and initializes a new <see cref="ThreadSafeList{TItem}"/>.
    /// </summary>
    /// <returns>New initialized <see cref="ThreadSafeList{TItem}"/>.</returns>
    public static ThreadSafeList<TItem> Create()
    {
      ThreadSafeList<TItem> result = new ThreadSafeList<TItem>();
      result.Initialize();
      return result;
    }
  }
}
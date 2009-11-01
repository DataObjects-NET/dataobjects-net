// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.20

using System;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Collections
{
  /// <summary>
  /// Type-based dictionary allowing to associate arbitrary values
  /// (or a set of values of different types) with type-based keys.
  /// Any operation on it is atomic.
  /// </summary>
  public struct TypeBasedDictionary
  {
    private TypeBasedDictionaryImplementation implementation;

    /// <summary>
    /// Gets the value by its key and value type.
    /// </summary>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TItem">Value type.</typeparam>
    /// <returns>Found value, or <see langword="default(TItem)"/>.</returns>
    public TItem GetValue<TKey, TItem>()
    {
      return implementation.GetValue<TKey, TItem>();
    }

    /// <summary>
    /// Sets the value associated with specified key and value type.
    /// </summary>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TItem">Value type.</typeparam>
    /// <param name="item">The value to set.</param>
    public void  SetValue<TKey, TItem>(TItem item)
    {
      lock (implementation) {
        implementation.SetValue<TKey, TItem>(item);
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
      Type implementationTypeDef = typeof (TypeBasedDictionaryImplementation<>);
      Type vadiatorType = TypeHelper.CreateDummyType("TypeBasedDictionaryVariator", typeof (object));
      implementation = (TypeBasedDictionaryImplementation)implementationTypeDef.Activate(new Type[] {vadiatorType}, null);
    }


    // Static constructor replacement
    
    /// <summary>
    /// Creates and initializes a new <see cref="TypeBasedDictionary"/>.
    /// </summary>
    /// <returns>New initialized <see cref="TypeBasedDictionary"/>.</returns>
    public static TypeBasedDictionary Create()
    {
      TypeBasedDictionary result = new TypeBasedDictionary();
      result.Initialize();
      return result;
    }
  }
}
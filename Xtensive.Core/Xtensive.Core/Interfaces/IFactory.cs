// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.06.24

namespace Xtensive.Core
{
  /// <summary>
  /// Describes a factory object that is capable of creating 
  /// <typeparamref name="TValue"/> instances 
  /// relying on <typeparamref name="TKey"/> instances.
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  /// <typeparam name="TValue">The type of the resulting value.</typeparam>
  public interface IFactory<TKey, TValue>
  {
    /// <summary>
    /// Creates the object of type <typeparamref name="TValue"/> by specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>Newly created instance of type <typeparamref name="TValue"/>.</returns>
    TValue Create(TKey key);
  }
}
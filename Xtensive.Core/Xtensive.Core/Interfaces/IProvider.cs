// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.28

namespace Xtensive.Core
{
  /// <summary>
  /// Provider contract.
  /// </summary>
  /// <typeparam name="TKey">The type of key.</typeparam>
  /// <typeparam name="TValue">The type of provided value.</typeparam>
  public interface IProvider<TKey, TValue>
  {
    /// <summary>
    /// Gets the instance of type <typeparamref name="TValue"/> 
    /// by its <paramref name="key"/>.
    /// </summary>
    TValue this[TKey key] { get; }
  }
}
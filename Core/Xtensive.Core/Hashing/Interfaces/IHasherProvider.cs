// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.15

using System;

namespace Xtensive.Hashing
{
  /// <summary>
  /// Hasher provider.
  /// </summary>
  public interface IHasherProvider
  {
    /// <summary>
    /// Gets <see cref="IHasher{T}"/> for the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">Type to get the hasher for.</typeparam>
    /// <returns><see cref="IHasher{T}"/> for the specified type <typeparamref name="T"/>.</returns>
    Hasher<T> GetHasher<T>();

    /// <summary>
    /// Gets <see cref="IHasherBase"/> for the specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Value to get the hasher for.</param>
    /// <returns><see cref="IHasherBase"/> for the specified <paramref name="value"/>.</returns>
    IHasherBase GetHasherByInstance(object value);

    /// <summary>
    /// Gets <see cref="IHasherBase"/> for the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Type to get the hasher for.</param>
    /// <returns><see cref="IHasherBase"/> for the specified <paramref name="type"/>.</returns>
    IHasherBase GetHasherByType(Type type);
  }
}
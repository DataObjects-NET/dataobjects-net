// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.01.14

using System;

namespace Xtensive.Hashing
{
  /// <summary>
  /// Calculates <see cref="long"/> hashes.
  /// </summary>
  /// <typeparam name="T">Type of object to calculate hash for.</typeparam>
  public interface IHasher<T>: IHasherBase
  {
    /// <summary>
    /// Calculates hash.
    /// </summary>
    /// <param name="value">Object to calculate hash to.</param>
    /// <returns>Hash.</returns>
    long GetHash(T value);

    /// <summary>
    /// Calculates <paramref name="count"/> of different hashes at once.
    /// </summary>
    /// <param name="value">Object to calculate hashes for.</param>
    /// <param name="count">Count of hashes to calculate.</param>
    /// <returns>Array of <paramref name="count"/> hashes.</returns>
    long[] GetHashes(T value, int count);
  }
}
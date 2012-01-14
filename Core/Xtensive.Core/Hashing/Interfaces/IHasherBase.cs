// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.22

namespace Xtensive.Hashing
{
  /// <summary>
  /// Tagging interface for any hasher supported by
  /// <see cref="HasherProvider"/>.
  /// </summary>
  public interface IHasherBase
  {
    /// <summary>
    /// Gets the provider this hasher is associated with.
    /// </summary>
    IHasherProvider Provider { get; }

    /// <summary>
    /// Calculates hash.
    /// </summary>
    /// <param name="value">Object to calculate hash to.</param>
    /// <returns>Hash.</returns>
    long GetInstanceHash(object value);
  }
}
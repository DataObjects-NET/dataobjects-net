// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.05.25

namespace Xtensive.Core
{
  /// <summary>
  /// Generates new identifiers.
  /// </summary>
  /// <typeparam name="TIdentifier">The type of identifiers to generate.</typeparam>
  public interface IIdentifierGenerator<TIdentifier>
  {
    /// <summary>
    /// Generates new identifier for the specified <typeparamref name="TIdentified"/> type.
    /// </summary>
    /// <typeparam name="TIdentified">A type of objects to be identified
    /// by the genrated identifier.</typeparam>
    /// <param name="isVolatile"><see langword="True"/> if generated identifier is volatile;
    /// otherwise, <see langword="false"/>.</param>
    /// <returns>Newly generated identifier.</returns>
    TIdentifier CreateNewIdentifier<TIdentified>(out bool isVolatile);
  }
}
// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.05.25

namespace Xtensive.Core
{
  /// <summary>
  /// Resolves identifiers - i.e. provides objects (identifier owners)
  /// having specified identifiers.
  /// </summary>
  /// <typeparam name="TIdentifier">The type of identifiers this resolver can handle.</typeparam>
  /// <typeparam name="TObject">The type of object this identifier resolver resolves identifiers to.</typeparam>
  public interface IIdentifierResolver<TIdentifier, TObject>
  {
    /// <summary>
    /// Resolves identifier by providing its owner, when identifier is <see cref="ITypedIdentifier"/>.
    /// </summary>
    /// <param name="identifier">Identifier to resolve.</param>
    /// <returns>Identifier owner.</returns>
    TObject Resolve(TIdentifier identifier);
  }
}
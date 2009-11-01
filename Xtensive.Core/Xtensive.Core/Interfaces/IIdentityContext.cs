// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.05.25

namespace Xtensive.Core
{
  /// <summary>
  /// Registers <see cref="IIdentified{TIdentifier}"/> objects and <see cref="IIdentifierContainer{TIdentifier}"/>s, 
  /// resolves identifiers, notifies <see cref="IIdentifierContainer{TIdentifier}"/>s on volatile
  /// identifiers change, generates new identifiers.
  /// </summary>
  /// <typeparam name="TObject">The type of objects storing identifiers inside the context.</typeparam>
  /// <typeparam name="TIdentifier">The type of identifiers this context can handle.</typeparam>
  public interface IIdentityContext<TIdentifier, TObject>: 
    IIdentifierResolver<TIdentifier, TObject>, IIdentifierGenerator<TIdentifier>
    where TObject : class, IIdentified<TIdentifier>
  {
    /// <summary>
    /// Registers identified object.
    /// </summary>
    /// <param name="identified">Identified object to register.</param>
    void Register(TObject identified);

    /// <summary>
    /// Unregisters identified object.
    /// </summary>
    /// <param name="identified">Identified object to unregister.</param>
    void Unregister(TObject identified);

    /// <summary>
    /// Raised when <see cref="IIdentifiedByVolatile">volatile</see> <see cref="IIdentified.Identifier"/> value is changed.
    /// </summary>
    /// <param name="changedObject">Object, which identifier is changed.</param>
    void RegisterIdentifierChange(TObject changedObject);

    /// <summary>
    /// Registers container of identifiers.
    /// </summary>
    /// <param name="container">Container of identifiers to register.</param>
    void RegisterContainer(IIdentifierContainer<TIdentifier> container);

    /// <summary>
    /// Unregisters container of identifiers.
    /// </summary>
    /// <param name="container">Container of identifiers to unregister.</param>
    void UnregisterContainer(IIdentifierContainer<TIdentifier> container);
  }
}
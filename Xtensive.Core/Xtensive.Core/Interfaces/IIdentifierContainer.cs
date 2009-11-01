// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.05.25

using System;
using System.Collections.Generic;
using Xtensive.Core.Collections;

namespace Xtensive.Core
{
  /// <summary>
  /// Raised when <see cref="IIdentifierContainer{TIdentifier}"/> content is changed.
  /// </summary>
  /// <param name="container">Object, which content is changed.</param>
  /// <param name="addedIdentifiers">Added identifiers. 
  /// Shouldn't be used if <paramref name="completelyChanged"/> is <see langword="true"/>.</param>
  /// <param name="removedIdentifiers">Removed identifiers.
  /// Shouldn't be used if <paramref name="completelyChanged"/> is <see langword="true"/>.</param>
  /// <param name="completelyChanged"><see langword="True"/>, if content is completely changed.</param>
  /// <typeparam name="TIdentifier">The type of contained identifiers.</typeparam>
  public delegate void IdentifierContainerChanged<TIdentifier>(IIdentifierContainer<TIdentifier> container,
    ISet<TIdentifier> addedIdentifiers, ISet<TIdentifier> removedIdentifiers,
    bool completelyChanged);


  /// <summary>
  /// Allows any identifier container to remap volatile identifiers
  /// on their change by interaction with <see cref="IIdentityContext{TObject, TIdentifier}"/>.
  /// </summary>
  /// <typeparam name="TIdentifier">The type of contained identifiers.</typeparam>
  public interface IIdentifierContainer<TIdentifier>
  {
    /// <summary>
    /// Gets a set of contained identifiers.
    /// </summary>
    ISet<TIdentifier> ContainedIdentifiers { get; }

    /// <summary>
    /// Gets a set of contained volatile identifiers (see <see cref="IIdentifiedByVolatile.IsIdentifierVolatile"/>).
    /// </summary>
    ISet<TIdentifier> ContainedVolatileIdentifiers { get; }

    /// <summary>
    /// Invoked by <see cref="IIdentityContext{TObject, TIdentifier}"/> when some 
    /// volatile identifiers of objects managed by it are changed
    /// (see <see cref="IIdentifiedByVolatile.IdentifierChanged"/>).
    /// </summary>
    /// <param name="map">Old-to-new identifier map.</param>
    void RemapVolatileIdentifiers(Dictionary<TIdentifier, TIdentifier> map);

    /// <summary>
    /// Invoked by <see cref="IIdentityContext{TObject, TIdentifier}"/> when some 
    /// identified objects are removed (unregistered) from
    /// the context.
    /// </summary>
    /// <param name="removedIdentifiers">A set of removed identifiers.</param>
    void RemoveIdentifiers(ISet<TIdentifier> removedIdentifiers);

    /// <summary>
    /// Raised when contained set of identifiers is changed.
    /// </summary>
    event IdentifierContainerChanged<TIdentifier> ContainedIdentifiersChanged;

    /// <summary>
    /// Raised when contained set of volatile identifiers is changed.
    /// </summary>
    event IdentifierContainerChanged<TIdentifier> ContainedVolatileIdentifiersChanged;
  }
}
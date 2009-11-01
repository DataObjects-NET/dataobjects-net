// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.05.25

namespace Xtensive.Core
{
  /// <summary>
  /// Implemented by objects having their identity context.
  /// </summary>
  /// <typeparam name="TIdentityContext">The type of identity context of the object.</typeparam>
  /// <typeparam name="TObject">The type of object stored in the itentity context.</typeparam>
  /// <typeparam name="TIdentifier">The type of identifier of identity context.</typeparam>
  public interface IIdentityContextBound<TIdentityContext, TObject, TIdentifier>
    where TIdentityContext: class, IIdentityContext<TIdentifier, TObject>
    where TObject: class, IIdentified<TIdentifier>
  {
    /// <summary>
    /// Gets <typeparamref name="TIdentityContext"/> of the object.
    /// </summary>
    TIdentityContext IdentityContext { get; }
  }
}
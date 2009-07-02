// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.05

using System;
using Xtensive.Core.Disposing;

namespace Xtensive.Storage
{
  /// <summary>
  /// Provides access to core services bound to a <see cref="Session"/>.
  /// </summary>
  public sealed class CoreServiceAccessor : SessionBound
  {
    /// <summary>
    /// Gets the accessor for <see cref="Persistent"/> descendants.
    /// </summary>
    public PersistentAccessor PersistentAccessor { get; private set; }

    /// <summary>
    /// Gets the accessor for <see cref="EntitySet{TItem}"/> descendants.
    /// </summary>
    public EntitySetAccessor EntitySetAccessor { get; private set; }

    /// <summary>
    /// Opens the region in which only the system logic will be executed.
    /// </summary>
    /// <returns>An object implementing <see cref="IDisposable"/> which 
    /// may be disposed to restore a previous state of the 
    /// <see cref="Session.SystemLogicOnly"/> property.</returns>
    public IDisposable OpenSystemLogicOnlyRegion()
    {
      var result = new Disposable<bool, Session>(Session.SystemLogicOnly, Session,
        (disposing, previousState, session) => session.SystemLogicOnly = previousState);
      Session.SystemLogicOnly = true;
      return result;
    }


    // Constructors

    /// <inheritdoc/>
    public CoreServiceAccessor(Session session)
      : base(session)
    {
      PersistentAccessor = new PersistentAccessor(session);
      EntitySetAccessor = new EntitySetAccessor(session);
    }
  }
}
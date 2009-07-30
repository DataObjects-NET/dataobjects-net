// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.05

using System;
using Xtensive.Core.Disposing;
using Xtensive.Storage.Providers;

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
    /// <see cref="Session.IsSystemLogicOnly"/> property.</returns>
    public IDisposable OpenSystemLogicOnlyRegion()
    {
      var result = new Disposable<Session, bool>(Session, Session.IsSystemLogicOnly,
        (disposing, session, previousState) => session.IsSystemLogicOnly = previousState);
      Session.IsSystemLogicOnly = true;
      return result;
    }

    /// <summary>
    /// Changes the value of <see cref="Session.Handler"/>.
    /// </summary>
    /// <param name="newHandler">The new handler.</param>
    /// <returns>An object implementing <see cref="IDisposable"/> which 
    /// may be disposed to restore a previous state of the 
    /// <see cref="Session.Handler"/> property.</returns>
    public IDisposable ChangeSessionHandler(SessionHandler newHandler)
    {
      var result = new Disposable<Session, SessionHandler>(Session, Session.Handler,
        (disposing, session, previousHandler) => session.Handler = previousHandler);
      Session.Handler = newHandler;
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
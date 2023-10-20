// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.05

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Services
{
  /// <summary>
  /// Provides access to core services bound to a <see cref="Session"/>.
  /// </summary>
  [Service(typeof(DirectSessionAccessor))]
  public sealed class DirectSessionAccessor : SessionBound,
    ISessionService
  {
    /// <summary>
    /// Opens the region in which only the system logic is executed.
    /// </summary>
    /// <returns>
    /// An object implementing <see cref="IDisposable"/> which
    /// disposal will restore previous state of
    /// <see cref="Session.IsSystemLogicOnly"/> property.
    /// </returns>
    public Session.SystemLogicOnlyRegionScope OpenSystemLogicOnlyRegion() => Session.OpenSystemLogicOnlyRegion();

    /// <summary>
    /// Changes the value of <see cref="Session.Handler"/>.
    /// </summary>
    /// <param name="newHandler">The new handler.</param>
    /// <returns>
    /// An object implementing <see cref="IDisposable"/> which
    /// disposal will restore previous state of
    /// <see cref="Session.Handler"/> property.
    /// </returns>
    public IDisposable ChangeSessionHandler(SessionHandler newHandler)
    {
      var result = new Disposable<Session, SessionHandler>(Session, Session.Handler,
        (disposing, session, oldHandler) => session.Handler = oldHandler);
      Session.Handler = newHandler;
      return result;
    }

    /// <summary>
    /// Sets the value of <see cref="Session.Transaction"/> to <see langword="null" />.
    /// </summary>
    /// <returns>
    /// An object implementing <see cref="IDisposable"/> which
    /// disposal will restore previous state of
    /// <see cref="Session.Transaction"/> property;
    /// <see langword="null" />, if <see cref="Session.Transaction"/>
    /// is already <see langword="null" />.
    /// </returns>
    public IDisposable NullifySessionTransaction()
    {
      var transaction = Session.Transaction;
      if (transaction==null)
        return null;
      var result = new Disposable<Session, Transaction>(Session, transaction,
        (disposing, session, oldTransaction) => session.SetTransaction(oldTransaction));
      Session.SetTransaction(null);
      return result;
    }

    /// <summary>
    /// Gets entities that were changed in the current session, but were not
    /// saved to the database yet.
    /// </summary>
    /// <param name="persistenceState">Type of entity change.</param>
    /// <returns><see cref="EntityState"/>s with the specified <paramref name="persistenceState"/>.</returns>
    public RegistryItems<EntityState> GetChangedEntities(PersistenceState persistenceState)
    {
      return Session.EntityChangeRegistry.GetItems(persistenceState);
    }


    // Constructors

    /// <inheritdoc/>
    [ServiceConstructor]
    public DirectSessionAccessor(Session session)
      : base(session)
    {
    }
  }
}
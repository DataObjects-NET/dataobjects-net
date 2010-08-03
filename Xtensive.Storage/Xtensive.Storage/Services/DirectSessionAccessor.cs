// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.05

using System;
using Xtensive.Core.Aspects;
using Xtensive.Core.Disposing;
using Xtensive.Core.IoC;
using Xtensive.Storage.Providers;

namespace Xtensive.Storage.Services
{
  /// <summary>
  /// Provides access to core services bound to a <see cref="Session"/>.
  /// </summary>
  [Service(typeof(DirectSessionAccessor))]
  [Infrastructure]
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
    public IDisposable OpenSystemLogicOnlyRegion()
    {
      return Session.OpenSystemLogicOnlyRegion();
    }

    /// <summary>
    /// Temporarily disables the operation logging.
    /// </summary>
    /// <returns>A disposable object defining the scope of this operation.</returns>
    public IDisposable DisableOperationLogging()
    {
      return Session.DisableOperationLogging();
    }

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
      newHandler.Session = Session;
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


    // Constructors

    /// <inheritdoc/>
    [ServiceConstructor]
    public DirectSessionAccessor(Session session)
      : base(session)
    {
    }
  }
}
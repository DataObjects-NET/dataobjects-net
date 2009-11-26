// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.26

using System;
using System.Transactions;
using Xtensive.Core;

namespace Xtensive.Storage
{
  partial class Transaction
  {
    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object, if new <see cref="Transaction"/> is created;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    public static TransactionScope Open()
    {
      var session = Session.Demand();
      return session.OpenTransaction(TransactionOpenMode.Default, session.Configuration.DefaultIsolationLevel);
    }

    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object, if new <see cref="Transaction"/> is created;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    public static TransactionScope Open(IsolationLevel isolationLevel)
    {
      var session = Session.Demand();
      return session.OpenTransaction(TransactionOpenMode.Default, isolationLevel);
    }
    
    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <param name="mode">The mode.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object, if new <see cref="Transaction"/> is created;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    public static TransactionScope Open(TransactionOpenMode mode)
    {
      var session = Session.Demand();
      return session.OpenTransaction(mode, session.Configuration.DefaultIsolationLevel);
    }

    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <param name="mode">The mode.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object, if new <see cref="Transaction"/> is created;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    public static TransactionScope Open(TransactionOpenMode mode, IsolationLevel isolationLevel)
    {
      var session = Session.Demand();
      return session.OpenTransaction(mode, isolationLevel);
    }

    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object, if new <see cref="Transaction"/> is created;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    public static TransactionScope Open(Session session)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      return session.OpenTransaction(TransactionOpenMode.Default, session.Configuration.DefaultIsolationLevel);
    }

    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object, if new <see cref="Transaction"/> is created;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    public static TransactionScope Open(Session session, IsolationLevel isolationLevel)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      return session.OpenTransaction(TransactionOpenMode.Default, isolationLevel);
    }

    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="mode">The mode.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object, if new <see cref="Transaction"/> is created;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    public static TransactionScope Open(Session session, TransactionOpenMode mode)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      return session.OpenTransaction(mode, session.Configuration.DefaultIsolationLevel);
    }

    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="mode">The mode.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object, if new <see cref="Transaction"/> is created;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    public static TransactionScope Open(Session session, TransactionOpenMode mode, IsolationLevel isolationLevel)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      return session.OpenTransaction(mode, isolationLevel);
    }

  }
}
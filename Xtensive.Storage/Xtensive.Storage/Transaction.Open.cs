// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.26

using System;
using System.Transactions;
using Xtensive.Core;
using Xtensive.Storage.Configuration;

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
      return session.OpenTransaction(TransactionOpenMode.Default, IsolationLevel.Unspecified);
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
      return session.OpenTransaction(mode, IsolationLevel.Unspecified);
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
      return session.OpenTransaction(TransactionOpenMode.Default, IsolationLevel.Unspecified);
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
      return session.OpenTransaction(mode, IsolationLevel.Unspecified);
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

    internal static TransactionScope HandleAutoTransaction(Session session, TransactionalBehavior behavior)
    {
      switch (behavior) {
        case TransactionalBehavior.Auto:
          if (session.IsDisconnected)
            goto case TransactionalBehavior.Open;
          if (session.Configuration.Behavior == SessionBehavior.Client)
            goto case TransactionalBehavior.Suppress;
          if (session.Configuration.Behavior == SessionBehavior.Server)
            goto case TransactionalBehavior.Require;
          goto default;
        case TransactionalBehavior.Require:
          Require(session);
          return TransactionScope.VoidScopeInstance;
        case TransactionalBehavior.Open:
          return Open(session, TransactionOpenMode.Auto);
        case TransactionalBehavior.New:
          return Open(session, TransactionOpenMode.New);
        case TransactionalBehavior.Suppress:
          return TransactionScope.VoidScopeInstance;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }
  }
}
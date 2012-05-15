// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.26

using System;
using System.Transactions;
using Xtensive.Core;
using Xtensive.Disposing;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm
{
  partial class Transaction
  {
    // Open method group

    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object. Its disposal will lead to either commit
    /// or rollback of the transaction it controls dependently on <see cref="CompletableScope.IsCompleted"/> flag.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    [Obsolete("Use Session.OpenTransaction() method instead")]
    public static TransactionScope Open()
    {
      var session = Session.Demand();
      return session.OpenTransaction(TransactionOpenMode.Default, IsolationLevel.Unspecified, false);
    }

    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object. Its disposal will lead to either commit
    /// or rollback of the transaction it controls dependently on <see cref="CompletableScope.IsCompleted"/> flag.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    [Obsolete("Use Session.OpenTransaction() method instead")]
    public static TransactionScope Open(IsolationLevel isolationLevel)
    {
      var session = Session.Demand();
      return session.OpenTransaction(TransactionOpenMode.Default, isolationLevel, false);
    }
    
    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <param name="mode">The mode.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object. Its disposal will lead to either commit
    /// or rollback of the transaction it controls dependently on <see cref="CompletableScope.IsCompleted"/> flag.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    [Obsolete("Use Session.OpenTransaction() method instead")]
    public static TransactionScope Open(TransactionOpenMode mode)
    {
      var session = Session.Demand();
      return session.OpenTransaction(mode, IsolationLevel.Unspecified, false);
    }

    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <param name="mode">The mode.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object. Its disposal will lead to either commit
    /// or rollback of the transaction it controls dependently on <see cref="CompletableScope.IsCompleted"/> flag.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    [Obsolete("Use Session.OpenTransaction() method instead")]
    public static TransactionScope Open(TransactionOpenMode mode, IsolationLevel isolationLevel)
    {
      var session = Session.Demand();
      return session.OpenTransaction(mode, isolationLevel, false);
    }

    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object. Its disposal will lead to either commit
    /// or rollback of the transaction it controls dependently on <see cref="CompletableScope.IsCompleted"/> flag.
    /// </returns>
    [Obsolete("Use Session.OpenTransaction() method instead")]
    public static TransactionScope Open(Session session)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      return session.OpenTransaction(TransactionOpenMode.Default, IsolationLevel.Unspecified, false);
    }

    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object. Its disposal will lead to either commit
    /// or rollback of the transaction it controls dependently on <see cref="CompletableScope.IsCompleted"/> flag.
    /// </returns>
    [Obsolete("Use Session.OpenTransaction() method instead")]
    public static TransactionScope Open(Session session, IsolationLevel isolationLevel)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      return session.OpenTransaction(TransactionOpenMode.Default, isolationLevel, false);
    }

    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="mode">The mode.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object. Its disposal will lead to either commit
    /// or rollback of the transaction it controls dependently on <see cref="CompletableScope.IsCompleted"/> flag.
    /// </returns>
    [Obsolete("Use Session.OpenTransaction() method instead")]
    public static TransactionScope Open(Session session, TransactionOpenMode mode)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      return session.OpenTransaction(mode, IsolationLevel.Unspecified, false);
    }

    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="mode">The mode.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object. Its disposal will lead to either commit
    /// or rollback of the transaction it controls dependently on <see cref="CompletableScope.IsCompleted"/> flag.
    /// </returns>
    [Obsolete("Use Session.OpenTransaction() method instead")]
    public static TransactionScope Open(Session session, TransactionOpenMode mode, IsolationLevel isolationLevel)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      return session.OpenTransaction(mode, isolationLevel, false);
    }


    // OpenAuto method group

    /// <summary>
    /// Opens the automatic transaction, or does nothing - dependently on specified
    /// behavior and <see cref="SessionOptions"/>.
    /// </summary>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object. Its disposal will lead to either commit
    /// or rollback of the transaction it controls dependently on <see cref="CompletableScope.IsCompleted"/> flag.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    [Obsolete("Use Session.OpenAutoTransaction() method instead")]
    public static TransactionScope OpenAuto()
    {
      var session = Session.Demand();
      return OpenAuto(session, TransactionalBehavior.Auto, IsolationLevel.Unspecified);
    }

    /// <summary>
    /// Opens the automatic transaction, or does nothing - dependently on specified
    /// behavior and <see cref="SessionOptions"/>.
    /// </summary>
    /// <param name="behavior">The automatic transaction behavior.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object. Its disposal will lead to either commit
    /// or rollback of the transaction it controls dependently on <see cref="CompletableScope.IsCompleted"/> flag.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    [Obsolete("Use Session.OpenAutoTransaction() method instead")]
    public static TransactionScope OpenAuto(TransactionalBehavior behavior)
    {
      var session = Session.Demand();
      return OpenAuto(session, behavior, IsolationLevel.Unspecified);
    }

    /// <summary>
    /// Opens the automatic transaction, or does nothing - dependently on specified
    /// behavior and <see cref="SessionOptions"/>.
    /// </summary>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object. Its disposal will lead to either commit
    /// or rollback of the transaction it controls dependently on <see cref="CompletableScope.IsCompleted"/> flag.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    [Obsolete("Use Session.OpenAutoTransaction() method instead")]
    public static TransactionScope OpenAuto(IsolationLevel isolationLevel)
    {
      var session = Session.Demand();
      return OpenAuto(session, TransactionalBehavior.Auto, isolationLevel);
    }

    /// <summary>
    /// Opens the automatic transaction, or does nothing - dependently on specified
    /// behavior and <see cref="SessionOptions"/>.
    /// </summary>
    /// <param name="behavior">The automatic transaction behavior.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object. Its disposal will lead to either commit
    /// or rollback of the transaction it controls dependently on <see cref="CompletableScope.IsCompleted"/> flag.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    [Obsolete("Use Session.OpenAutoTransaction() method instead")]
    public static TransactionScope OpenAuto(TransactionalBehavior behavior, IsolationLevel isolationLevel)
    {
      var session = Session.Demand();
      return OpenAuto(session, behavior, isolationLevel);
    }

    /// <summary>
    /// Opens the automatic transaction, or does nothing - dependently on specified
    /// behavior and <see cref="SessionOptions"/>.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object. Its disposal will lead to either commit
    /// or rollback of the transaction it controls dependently on <see cref="CompletableScope.IsCompleted"/> flag.
    /// </returns>
    [Obsolete("Use Session.OpenAutoTransaction() method instead")]
    public static TransactionScope OpenAuto(Session session)
    {
      return OpenAuto(session, TransactionalBehavior.Auto, IsolationLevel.Unspecified);
    }

    /// <summary>
    /// Opens the automatic transaction, or does nothing - dependently on specified
    /// behavior and <see cref="SessionOptions"/>.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="behavior">The automatic transaction behavior.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object. Its disposal will lead to either commit
    /// or rollback of the transaction it controls dependently on <see cref="CompletableScope.IsCompleted"/> flag.
    /// </returns>
    [Obsolete("Use Session.OpenAutoTransaction() method instead")]
    public static TransactionScope OpenAuto(Session session, TransactionalBehavior behavior)
    {
      return OpenAuto(session, behavior, IsolationLevel.Unspecified);
    }

    /// <summary>
    /// Opens the automatic transaction, or does nothing - dependently on specified
    /// behavior and <see cref="SessionOptions"/>.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object. Its disposal will lead to either commit
    /// or rollback of the transaction it controls dependently on <see cref="CompletableScope.IsCompleted"/> flag.
    /// </returns>
    [Obsolete("Use Session.OpenAutoTransaction() method instead")]
    public static TransactionScope OpenAuto(Session session, IsolationLevel isolationLevel)
    {
      return OpenAuto(session, TransactionalBehavior.Auto, isolationLevel);
    }

    /// <summary>
    /// Opens the automatic transaction, or does nothing - dependently on specified
    /// behavior and <see cref="SessionOptions"/>.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="behavior">The automatic transaction behavior.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object. Its disposal will lead to either commit
    /// or rollback of the transaction it controls dependently on <see cref="CompletableScope.IsCompleted"/> flag.
    /// </returns>
    [Obsolete("Use Session.OpenAutoTransaction() method instead")]
    public static TransactionScope OpenAuto(Session session, TransactionalBehavior behavior, IsolationLevel isolationLevel)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      switch (behavior) {
        case TransactionalBehavior.Auto:
          if (session.Configuration.Supports(SessionOptions.AutoTransactionOpenMode))
            goto case TransactionalBehavior.Open;
          if (session.Configuration.Supports(SessionOptions.AutoTransactionSuppressMode))
            goto case TransactionalBehavior.Suppress;
          goto case TransactionalBehavior.Require;
        case TransactionalBehavior.Require:
          Require(session);
          return TransactionScope.VoidScopeInstance;
        case TransactionalBehavior.Open:
          if (session.IsDisconnected && session.Transaction!=null && !session.Transaction.IsDisconnected)
            goto case TransactionalBehavior.New;
          return session.OpenTransaction(TransactionOpenMode.Auto, isolationLevel, true);
        case TransactionalBehavior.New:
          return session.OpenTransaction(TransactionOpenMode.New, isolationLevel, true);
        case TransactionalBehavior.Suppress:
          return TransactionScope.VoidScopeInstance;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }
  }
}
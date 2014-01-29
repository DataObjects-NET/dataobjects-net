// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.26

using System;
using System.Transactions;
using Xtensive.Core;

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
    /// or rollback of the transaction it controls dependently on <see cref="ICompletableScope.IsCompleted"/> flag.
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
    /// or rollback of the transaction it controls dependently on <see cref="ICompletableScope.IsCompleted"/> flag.
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
    /// or rollback of the transaction it controls dependently on <see cref="ICompletableScope.IsCompleted"/> flag.
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
    /// or rollback of the transaction it controls dependently on <see cref="ICompletableScope.IsCompleted"/> flag.
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
    /// or rollback of the transaction it controls dependently on <see cref="ICompletableScope.IsCompleted"/> flag.
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
    /// or rollback of the transaction it controls dependently on <see cref="ICompletableScope.IsCompleted"/> flag.
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
    /// or rollback of the transaction it controls dependently on <see cref="ICompletableScope.IsCompleted"/> flag.
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
    /// or rollback of the transaction it controls dependently on <see cref="ICompletableScope.IsCompleted"/> flag.
    /// </returns>
    [Obsolete("Use Session.OpenTransaction() method instead")]
    public static TransactionScope Open(Session session, TransactionOpenMode mode, IsolationLevel isolationLevel)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      return session.OpenTransaction(mode, isolationLevel, false);
    }
  }
}
// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.20

using System;
using System.Transactions;
using Xtensive.Core;
using Xtensive.Core.Disposing;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Integrity.Transactions;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Rse.Providers.Executable;

namespace Xtensive.Storage
{
  /// <summary>
  /// An implementation of transaction suitable for storage.
  /// </summary>
  public sealed class Transaction : TransactionBase
  {
    private IDisposable inconsistentRegion;

    /// <summary>
    /// Gets the session.
    /// </summary>
    public Session Session { get; private set; }

    /// <summary>
    /// Gets the transaction-level temporary data.
    /// </summary>
    public TransactionTemporaryData TemporaryData { get; private set; }

    /// <summary>
    /// Gets the validation context of this <see cref="Transaction"/>.
    /// </summary>    
    public ValidationContext ValidationContext { get; private set; }

    /// <inheritdoc/>
    protected override Integrity.Transactions.TransactionScope CreateScope()
    {
      return new TransactionScope(this);
    }

    #region OnXxx methods

    /// <inheritdoc/>
    protected override void OnBegin()
    {
      ValidationContext.Reset();
      if (Session.Domain.Configuration.InconsistentTransactions)
        inconsistentRegion = ValidationContext.OpenInconsistentRegion();
      Session.BeginTransaction();
    }

    /// <inheritdoc/>
    protected override void OnCommit()
    {
      try {
        inconsistentRegion.DisposeSafely();
        if (!ValidationContext.IsValid)
          throw new InvalidOperationException(Strings.ExCanNotCommitATransactionValidationContextIsInInvalidState);
        if (!ValidationContext.IsConsistent)
          throw new InvalidOperationException(Strings.ExCannotCommitATransactionValidationContextIsInInconsistentState);
      }
      catch {
        OnRollback();
        throw;
      }      
      Session.CommitTransaction();
    }

    /// <inheritdoc/>
    protected override void OnRollback()
    {
      try {
        inconsistentRegion.DisposeSafely();
      }
      finally {
        Session.RollbackTransaction();
      }
    }

    #endregion

    #region Static Current (property), Open (method)

    /// <summary>
    /// Gets the current <see cref="Transaction"/> object
    /// using <see cref="Session"/>.<see cref="Storage.Session.Current"/>.
    /// </summary>
    public static Transaction Current {
      get {
        var session = Session.Current;
        return session!=null ? session.Transaction : null;
      }
    }

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
      return Session.Demand().OpenTransaction();
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
      return Session.Demand().OpenTransaction(isolationLevel);
    }

    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <param name="autoTransaction">if set to <see langword="true"/> auto transaction is demanded.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object, if new <see cref="Transaction"/> is created;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    public static TransactionScope Open(bool autoTransaction)
    {
      return Session.Demand().OpenTransaction(autoTransaction);
    }

    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <param name="autoTransaction">if set to <see langword="true"/> auto transaction is demanded.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object, if new <see cref="Transaction"/> is created;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    public static TransactionScope Open(IsolationLevel isolationLevel, bool autoTransaction)
    {
      return Session.Demand().OpenTransaction(isolationLevel, autoTransaction);
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
      return session.OpenTransaction();
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
      return session.OpenTransaction(isolationLevel);
    }

    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="autoTransaction">if set to <see langword="true"/> auto transaction is demanded.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object, if new <see cref="Transaction"/> is created;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    public static TransactionScope Open(Session session, bool autoTransaction)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      return session.OpenTransaction(autoTransaction);
    }

    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <param name="autoTransaction">if set to <see langword="true"/> auto transaction is demanded.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object, if new <see cref="Transaction"/> is created;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    public static TransactionScope Open(Session session, IsolationLevel isolationLevel, bool autoTransaction)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      return session.OpenTransaction(isolationLevel, autoTransaction);
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    internal Transaction(Session session, IsolationLevel isolationLevel)
      : this (session, Guid.NewGuid(), isolationLevel)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="identifier">The identifier.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    internal Transaction(Session session, Guid identifier, IsolationLevel isolationLevel)
      : base (identifier, isolationLevel)
    {
      Session = session;
      TemporaryData = new TransactionTemporaryData();
      ValidationContext = new ValidationContext();
    }
  }
} 

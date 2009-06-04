// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.20

using System;
using System.Transactions;
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
      if (Session.Domain.Configuration.InconsistentTransactions)
        inconsistentRegion = ValidationContext.OpenInconsistentRegion();
      Session.BeginTransaction();
    }

    /// <inheritdoc/>
    protected override void OnCommit()
    {
      try {
        inconsistentRegion.DisposeSafely();
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
    /// Does the same as <see cref="Storage.Session.OpenTransaction(IsolationLevel)"/>,
    /// but for the <see cref="Current"/> transaction.
    /// </summary>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object, if new <see cref="Transaction"/> is created;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    public static TransactionScope Open(IsolationLevel isolationLevel)
    {
      return DemandSession().OpenTransaction(isolationLevel);
    }

    /// <summary>
    /// Does the same as <see cref="Storage.Session.OpenTransaction()"/>,
    /// but for the <see cref="Current"/> transaction.
    /// </summary>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object, if new <see cref="Transaction"/> is created;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    public static TransactionScope Open()
    {
      return DemandSession().OpenTransaction();
    }

    /// <summary>
    /// Does the same as <see cref="Storage.Session.OpenTransaction(bool)"/>,
    /// but for the <see cref="Current"/> transaction.
    /// </summary>
    /// <param name="autoTransaction">if set to <see langword="true"/> auto transaction is demanded.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object, if new <see cref="Transaction"/> is created;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    public static TransactionScope Open(bool autoTransaction)
    {
      return DemandSession().OpenTransaction(autoTransaction);
    }

    /// <summary>
    /// Does the same as <see cref="Storage.Session.OpenTransaction(bool)"/>,
    /// but for the <see cref="Current"/> transaction.
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
      return DemandSession().OpenTransaction(isolationLevel, autoTransaction);
    }

    #endregion

    #region Private / internal methods

    private static Session DemandSession()
    {
      var session = Session.Current;
      if (session == null)
        throw new InvalidOperationException(Strings.ExCanNotOpenTransactionNoCurrentSession);
      return session;
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

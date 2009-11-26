// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.20

using System;
using System.Transactions;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Disposing;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Integrity.Transactions;
using Xtensive.Integrity.Validation;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Rse.Providers.Executable;

namespace Xtensive.Storage
{
  /// <summary>
  /// An implementation of transaction suitable for storage.
  /// </summary>
  public sealed class Transaction : TransactionBase,
    IHasExtensions
  {
    private InconsistentRegion inconsistentRegion;
    private ExtensionCollection extensions;
    
    /// <summary>
    /// Gets the session.
    /// </summary>
    public Session Session { get; private set; }

    /// <summary>
    /// Gets the outer transaction.
    /// </summary>
    public Transaction Outer { get; private set; }

    /// <summary>
    /// Gets the outermost transaction.
    /// </summary>
    public Transaction Outermost { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this transaction is a nested transaction.
    /// </summary>
    public bool IsNested { get { return Outer!=null; } }

    /// <summary>
    /// Gets the transaction-level temporary data.
    /// </summary>
    public TransactionTemporaryData TemporaryData { get; private set; }

    /// <summary>
    /// Gets the validation context of this <see cref="Transaction"/>.
    /// </summary>    
    public ValidationContext ValidationContext { get; private set; }

    internal string SavepointName { get; private set; }

    internal Transaction Inner { get; set; }

    internal bool IsActuallyStarted { get; set; }
    
    /// <inheritdoc/>
    protected override Integrity.Transactions.TransactionScope CreateScope()
    {
      return new TransactionScope(this);
    }

    internal bool AreChangesVisibleTo(Transaction otherTransaction)
    {
      ArgumentValidator.EnsureArgumentNotNull(otherTransaction, "otherTransaction");
      if (Outermost!=otherTransaction.Outermost)
        return false;
      var t = this;
      var outermost = t.Outermost;
      while (t!=outermost && t!=otherTransaction && t.State==TransactionState.Committed)
        t = t.Outer;
      return t.State.IsActive();      
    }

    #region IHasExtensions Members

    /// <inheritdoc/>
    public IExtensionCollection Extensions {
      get {
        if (extensions != null)
          return extensions;

        lock (this) {
          if (extensions == null)
            extensions = new ExtensionCollection();
        }

        return extensions;
      }
    }

    #endregion

    #region OnXxx methods

    /// <inheritdoc/>
    protected override void OnBegin()
    {
      ValidationContext.Reset();
      if (Session.Domain.Configuration.ValidationMode==ValidationMode.OnDemand)
        inconsistentRegion = ValidationContext.OpenInconsistentRegion();
      Session.BeginTransaction(this);
    }

    /// <inheritdoc/>
    protected override void OnCommit()
    {
      try {
        if (inconsistentRegion==null && !ValidationContext.IsConsistent)
          throw new InvalidOperationException(Strings.ExCanNotCommitATransactionValidationContextIsInInconsistentState);

        try {
          Validation.Enforce(Session);

          if (inconsistentRegion!=null) {
            inconsistentRegion.Complete();
            inconsistentRegion.DisposeSafely();
          }
        }
        catch (AggregateException exception) {
          throw new InvalidOperationException(Strings.ExCanNotCommitATransactionEntitiesValidationFailed, exception);
        }
      }
      catch {
        OnRollback();
        throw;
      }

      Session.CommitTransaction(this);
    }

    /// <inheritdoc/>
    protected override void OnRollback()
    {
      try {
        inconsistentRegion.DisposeSafely();
      }
      finally {
        Session.RollbackTransaction(this);
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

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    internal Transaction(Session session, IsolationLevel isolationLevel)
      : this(session, isolationLevel, null, null)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <param name="outer">The outer transaction.</param>
    /// <param name="savepointName">Name of the savepoint associated with nested transaction.</param>
    internal Transaction(Session session, IsolationLevel isolationLevel, Transaction outer, string savepointName)
      : base (Guid.NewGuid(), isolationLevel)
    {
      Session = session;
      TemporaryData = new TransactionTemporaryData();
      ValidationContext = new ValidationContext();

      if (outer!=null) {
        outer.Inner = this;
        Outer = outer;
        Outermost = outer.Outermost;
        SavepointName = savepointName;
      }
      else
        Outermost = this;
    }
  }
} 

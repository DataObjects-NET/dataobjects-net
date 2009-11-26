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
using Xtensive.Integrity.Transactions;
using Xtensive.Integrity.Validation;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Rse.Providers.Executable;

namespace Xtensive.Storage
{
  /// <summary>
  /// An implementation of transaction suitable for storage.
  /// </summary>
  public sealed partial class Transaction : IHasExtensions
  {
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

    private InconsistentRegion inconsistentRegion;
    private ExtensionCollection extensions;
    private Transaction inner;
    
    /// <summary>
    /// Gets the session this transaction is bound to.
    /// </summary>
    public Session Session { get; private set; }

    /// <summary>
    /// Gets the isolation level.
    /// </summary>
    public IsolationLevel IsolationLevel { get; private set; }

    /// <summary>
    /// Gets the state of the transaction.
    /// </summary>
    public TransactionState State { get; private set; }

    /// <summary>
    /// Gets the outer transaction.
    /// </summary>
    public Transaction Outer { get; private set; }

    /// <summary>
    /// Gets the outermost transaction.
    /// </summary>
    public Transaction Outermost { get; private set; }

    /// <summary>
    /// Gets the timestamp of this transaction.
    /// </summary>
    public DateTime TimeStamp { get; private set; }
    
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
    
    #region IHasExtensions Members

    /// <inheritdoc/>
    public IExtensionCollection Extensions {
      get {
        if (extensions==null)
          extensions = new ExtensionCollection();
        return extensions;
      }
    }

    #endregion

    internal string SavepointName { get; private set; }
    
    internal bool IsActuallyStarted { get; set; }
  
    #region Private / internal methods
    
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

    internal void Begin()
    {
      if (State!=TransactionState.NotActivated)
        throw new InvalidOperationException(Strings.ExTransactionShouldNotBeActive);
      try {
        PerformBegin();
      }
      catch {
        ClearReferences();
        throw;
      }
      State = TransactionState.Active;
    }

    internal void Commit()
    {
      EnsureTransactionIsActive();
      State = TransactionState.Committing;
      try {
        PerformCommit();
        State = TransactionState.Committed;
      }
      catch {
        State = TransactionState.RolledBack;
        throw;
      }
      finally {
        ClearReferences();
      }
    }

    internal void Rollback()
    {
      EnsureTransactionIsActive();
      State = TransactionState.RollingBack;
      try {
        PerformRollback();
      }
      finally {
        State = TransactionState.RolledBack;
        ClearReferences();
      }
    }

    private void EnsureTransactionIsActive()
    {
      if (State!=TransactionState.Active)
        throw new InvalidOperationException(Strings.ExTransactionIsNotActive);
    }

    private void PerformBegin()
    {
      BeginValidation();
      Session.BeginTransaction(this);
    }

    private void PerformCommit()
    {
      try {
        if (inner!=null)
          throw new InvalidOperationException(
            Strings.ExCanNotCompleteOuterTransactionInnerTransactionIsActive);
        CompleteValidation();
      }
      catch {
        PerformRollback();
        throw;
      }
      Session.CommitTransaction(this);
    }

    private void PerformRollback()
    {
      try {
        if (inner!=null)
          inner.Rollback();
        AbortValidation();
      }
      finally {
        Session.RollbackTransaction(this);
      }
    }

    private void ClearReferences()
    {
      if (Outer!=null)
        Outer.inner = null;
    }

    #endregion

    
    // Constructors

    internal Transaction(Session session, IsolationLevel isolationLevel)
      : this(session, isolationLevel, null, null)
    {
    }

    internal Transaction(Session session, IsolationLevel isolationLevel, Transaction outer, string savepointName)
    {
      Session = session;
      IsolationLevel = isolationLevel;
      TimeStamp = DateTime.Now;
      TemporaryData = new TransactionTemporaryData();
      ValidationContext = new ValidationContext();
      
      if (outer!=null) {
        if (outer.inner!=null)
          throw new InvalidOperationException(Strings.ExCanNotOpenMoreThanOneInnerTransaction);
        outer.inner = this;
        Outer = outer;
        Outermost = outer.Outermost;
        SavepointName = savepointName;
      }
      else
        Outermost = this;
    }
  }
} 

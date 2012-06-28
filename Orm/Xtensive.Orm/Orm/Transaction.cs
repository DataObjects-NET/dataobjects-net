// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.20

using System;
using System.Transactions;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Validation;
using Xtensive.Orm.Rse.Providers;

namespace Xtensive.Orm
{
  /// <summary>
  /// An implementation of transaction suitable for storage.
  /// </summary>
  public sealed partial class Transaction : IHasExtensions
  {
    #region Current, Demand() members (static)

    /// <summary>
    /// Gets the current <see cref="Transaction"/> object
    /// using <see cref="Session"/>.<see cref="Orm.Session.Current"/>.
    /// </summary>
    public static Transaction Current {
      get {
        var session = Session.Current;
        return session != null 
          ? session.Transaction
          : null;
      }
    }

    /// <summary>
    /// Gets the current <see cref="Transaction"/>, 
    /// or throws <see cref="InvalidOperationException"/>, 
    /// if active <see cref="Transaction"/> is not found.
    /// </summary>
    /// <returns>Current transaction.</returns>
    /// <exception cref="InvalidOperationException"><see cref="Transaction.Current"/> <see cref="Transaction"/> is <see langword="null" />.</exception>
    public static Transaction Demand()
    {
      var currentTransaction = Current;
      if (currentTransaction==null)
        throw new InvalidOperationException(
          Strings.ExActiveTransactionIsRequiredForThisOperationUseTransactionOpenToOpenIt);
      return currentTransaction;
    }

    /// <summary>
    /// Checks whether a transaction exists or not in the provided session.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <exception cref="InvalidOperationException"><see cref="Transaction.Current"/> <see cref="Transaction"/> is <see langword="null" />.</exception>
    public static void Require(Session session)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      if (session.Transaction != null)
        return;
      throw new InvalidOperationException(
          Strings.ExActiveTransactionIsRequiredForThisOperationUseTransactionOpenToOpenIt);
    }

    #endregion

    private ICompletableScope inconsistentRegion;
    private ExtensionCollection extensions;
    private Transaction inner;

    /// <summary>
    /// Gets a value indicating whether this instance is automatic transaction.
    /// </summary>
    public bool IsAutomatic { get; private set; }
    
    /// <summary>
    /// Gets a value indicating whether this instance is 
    /// transaction running locally in <see cref="DisconnectedState"/>.
    /// </summary>
    public bool IsDisconnected { get; private set; }
    
    /// <summary>
    /// Gets the unique identifier of this transaction.
    /// Nested transactions have the same <see cref="Guid"/> 
    /// as their outermost.
    /// </summary>
    public Guid Guid { get; private set; }

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
    /// Gets the start time of this transaction.
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

    /// <summary>
    /// Indicates whether changes made in this transaction are visible "as is"
    /// in <paramref name="otherTransaction"/>. This implies <paramref name="otherTransaction"/>
    /// and this transaction at least share the same <see cref="Outermost"/> transaction.
    /// Please refer to the code of this method to clearly understand what it really does ;)
    /// </summary>
    /// <param name="otherTransaction">The other transaction.</param>
    /// <returns>
    /// <see langword="True"/> if changes made in this transaction are visible
    /// "as is" in <paramref name="otherTransaction"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool AreChangesVisibleTo(Transaction otherTransaction)
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
  
    #region Private / internal methods
    
    internal void Begin()
    {
      BeginValidation();
      Session.BeginTransaction(this);
      if (Outer!=null)
        Outer.inner = this;
      State = TransactionState.Active;
    }

    internal void Commit()
    {
      EnsureIsActive();
      State = TransactionState.Committing;
      try {
        if (inner!=null)
          throw new InvalidOperationException(
            Strings.ExCanNotCompleteOuterTransactionInnerTransactionIsActive);
        CompleteValidation();
        Session.CommitTransaction(this);
      }
      catch {
        Rollback();
        throw;
      }
      State = TransactionState.Committed;
      EndTransaction();
    }

    internal void Rollback()
    {
      EnsureIsActive();
      State = TransactionState.RollingBack;
      try {
        try {
          if (inner!=null)
            inner.Rollback();
        }
        finally {
          AbortValidation();
          Session.RollbackTransaction(this);
        }
      }
      finally {
        State = TransactionState.RolledBack;
        EndTransaction();
      }
    }

    private void EndTransaction()
    {
      if (Outer!=null)
        Outer.inner = null;
      Session.CompleteTransaction(this);
    }

    private void EnsureIsActive()
    {
      if (!State.IsActive())
        throw new InvalidOperationException(Strings.ExTransactionIsNotActive);
    }

    #endregion

    
    // Constructors

    internal Transaction(Session session, IsolationLevel isolationLevel, bool isAutomatic)
      : this(session, isolationLevel, isAutomatic, null, null)
    {
    }

    internal Transaction(Session session, IsolationLevel isolationLevel, bool isAutomatic, Transaction outer, string savepointName)
    {
      Guid = Guid.NewGuid();
      State = TransactionState.NotActivated;
      Session = session;
      IsolationLevel = isolationLevel;
      IsAutomatic = isAutomatic;
      IsDisconnected = session.IsDisconnected;
      TimeStamp = DateTime.UtcNow;
      TemporaryData = new TransactionTemporaryData();
      ValidationContext = new ValidationContext();
      
      if (outer!=null) {
        Outer = outer;
        Guid = outer.Guid;
        Outermost = outer.Outermost;
        SavepointName = savepointName;
      }
      else
        Outermost = this;
    }
  }
} 

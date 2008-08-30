// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.14

using System;
using System.Diagnostics;
using System.Transactions;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Integrity.Resources;

namespace Xtensive.Integrity.Transactions
{
  /// <summary>
  /// Base class for any transaction.
  /// </summary>
  public abstract class TransactionBase : ITransaction    
  {
    private readonly Guid identifier;
    private IsolationLevel isolationLevel;
    private TransactionScope scope;

    #region IIdentified<Guid> Members

    /// <inheritdoc/>
    public Guid Identifier {
      [DebuggerStepThrough]
      get { return identifier; }
    }

    /// <inheritdoc/>
    object IIdentified.Identifier {
      [DebuggerStepThrough]
      get { return identifier; }
    }

    #endregion

    /// <inheritdoc/>
    public TransactionState State { get; private set; }

    /// <inheritdoc/>
    public IsolationLevel IsolationLevel {
      [DebuggerStepThrough]
      get { return isolationLevel; }
    }

    /// <summary>
    /// Begins this transaction.
    /// </summary>
    /// <returns>Scope of this transaction.</returns>
    public TransactionScope Begin()
    {
      if (State!=TransactionState.NotActivated)
        throw new InvalidOperationException(Strings.ExTransactionIsAlreadyActivated);
      OnActivate();
      State = TransactionState.Active;
      scope = CreateScope();
      return scope;
    }

    /// <summary>
    /// Creates the scope for this transaction.
    /// </summary>
    /// <returns>Created scope.</returns>
    protected abstract TransactionScope CreateScope();

    #region Commit, Rollback

    void ITransaction.Commit()
    {
      throw new NotSupportedException(
        Strings.ExScopeBoundTransactionCanBeCommittedOnlyByItsScope);
    }

    /// <summary>
    /// Commits the transaction.
    /// </summary>
    internal void Commit()
    {
      if (State != TransactionState.Active)
        throw new InvalidOperationException(String.Format(
          Strings.ExInvalidTransactionState, State, TransactionState.Active));
      State = TransactionState.Committing;
      try {
        OnCommit();
        State = TransactionState.Committed;
      }
      catch {
        State = TransactionState.RolledBack;
        throw;
      }
    }

    /// <summary>
    /// Rolls back the transaction.
    /// </summary>
    public void Rollback()
    {
      if (State != TransactionState.Active)
        throw new InvalidOperationException(String.Format(
          Strings.ExInvalidTransactionState, State, TransactionState.Active));
      State = TransactionState.RollingBack;
      try {
        OnRollback();
      }
      finally {
        State = TransactionState.RolledBack;
      }
    }

    #endregion

    /// <summary>
    /// Called when transaction is being activated.
    /// </summary>
    protected abstract void OnActivate();

    /// <summary>
    /// Called when transaction is about to commit.
    /// </summary>
    protected abstract void OnCommit();

    /// <summary>
    /// Called when transaction is about to rollback.
    /// </summary>
    protected abstract void OnRollback();

    
    // Constructors
    
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="identifier">The identifier.</param>
    protected TransactionBase(Guid identifier)
    {
      this.identifier = identifier;
      State = TransactionState.NotActivated;
    }
  }
}
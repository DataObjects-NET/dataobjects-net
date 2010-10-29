// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.14

using System;
using System.Diagnostics;
using System.Transactions;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Resources;

namespace Xtensive.Transactions
{
  /// <summary>
  /// Base class for any transaction.
  /// </summary>
  public abstract class TransactionBase : ITransaction
  {
    private readonly Guid identifier;
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
    public IsolationLevel IsolationLevel { get; protected set; }

    /// <inheritdoc/>
    public DateTime TimeStamp { get; private set; }

    /// <summary>
    /// Begins this transaction.
    /// </summary>
    /// <returns>Scope of this transaction.</returns>
    public TransactionScope Begin()
    {
      if (State!=TransactionState.NotActivated)
        throw new InvalidOperationException(Strings.ExTransactionIsAlreadyActivated);
      OnBegin();
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
    /// Called when transaction is beginning.
    /// </summary>
    protected abstract void OnBegin();

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
    /// <param name="isolationLevel">The isolation level.</param>
    protected TransactionBase(Guid identifier, IsolationLevel isolationLevel)
    {
      IsolationLevel = isolationLevel;
      this.identifier = identifier;
      State = TransactionState.NotActivated;
      TimeStamp = DateTime.Now;
    }
  }
}
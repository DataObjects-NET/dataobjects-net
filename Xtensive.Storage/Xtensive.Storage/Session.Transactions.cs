// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.11.07

using Xtensive.Core.Disposable;

namespace Xtensive.Storage
{
  public partial class Session
  {
    /// <summary>
    /// Gets the active transaction.
    /// </summary>    
    public Transaction Transaction { get; private set; }

    /// <summary>
    /// Gets the ambient transaction scope.
    /// </summary>    
    public TransactionScope AmbientTransactionScope { get; private set; }

    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object, if new
    /// <see cref="Transaction"/> is created;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    public TransactionScope OpenTransaction()
    {
      var transaction = Transaction;
      if (transaction==null) {
        transaction = new Transaction(this);
        Transaction = transaction;
        var ts = (TransactionScope) transaction.Begin();
        if (ts!=null && Configuration.UsesAmbientTransactions) {
          AmbientTransactionScope = ts;
          return null;
        }
        return ts;
      }
      return null;
    }

    /// <summary>
    /// Commits the ambient transaction - 
    /// i.e. completes <see cref="AmbientTransactionScope"/> and disposes it.
    /// </summary>
    public void CommitAmbientTransaction()
    {
      var ts = AmbientTransactionScope;
      try {
        ts.Complete();
      }
      finally {
        AmbientTransactionScope = null;
        ts.DisposeSafely();
      }
    }

    /// <summary>
    /// Rolls back the ambient transaction - 
    /// i.e. disposes <see cref="AmbientTransactionScope"/>.
    /// </summary>
    public void RollbackAmbientTransaction()
    {
      var ts = AmbientTransactionScope;
      AmbientTransactionScope = null;
      ts.DisposeSafely();
    }

    #region OnXxx event-like methods

    internal void OnBeginTransaction()
    {
      Handler.BeginTransaction();
    }

    internal void OnCommitTransaction()
    {
      try {
        Persist();
        Handler.CommitTransaction();
        OnCompleteTransaction();
      }
      catch {        
        OnRollbackTransaction();
        throw;
      }
    }

    internal void OnRollbackTransaction()
    {
      try {
        Handler.RollbackTransaction();
      }
      finally {
        foreach (var item in EntityStateRegistry.GetItems(PersistenceState.New))
          item.PersistenceState = PersistenceState.Synchronized;
        foreach (var item in EntityStateRegistry.GetItems(PersistenceState.Modified))
          item.PersistenceState = PersistenceState.Synchronized;
        foreach (var item in EntityStateRegistry.GetItems(PersistenceState.Removed))
          item.PersistenceState = PersistenceState.Synchronized;
        EntityStateRegistry.Clear();
        OnCompleteTransaction();
      }
    }

    private void OnCompleteTransaction()
    {
      Transaction = null;
    }

    #endregion
  }
}
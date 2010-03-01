// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.19

using System.Transactions;

namespace Xtensive.Storage.Providers
{
  partial class SessionHandler
  {
    /// <summary>
    /// Gets a value indicating whether transaction is actually started.
    /// This indicates presence of outermost transaction only.
    /// </summary>
    public abstract bool TransactionIsStarted { get; }

    /// <summary>
    /// Opens the transaction.
    /// </summary>
    public abstract void BeginTransaction(IsolationLevel isolationLevel);

    /// <summary>
    /// Makes the savepoint.
    /// </summary>
    /// <param name="name">The name.</param>
    public abstract void MakeSavepoint(string name);

    /// <summary>
    /// Rollbacks to savepoint.
    /// </summary>
    /// <param name="name">The name.</param>
    public virtual void RollbackToSavepoint(string name)
    {
      prefetchManager.Clear();
    }

    /// <summary>
    /// Releases the savepoint.
    /// </summary>
    /// <param name="name">The name.</param>
    public virtual void ReleaseSavepoint(string name)
    {
      prefetchManager.Clear();
    }

    /// <summary>
    /// Commits the transaction.
    /// </summary>    
    public virtual void CommitTransaction()
    {
      prefetchManager.Clear();
    }

    /// <summary>
    /// Rollbacks the transaction.
    /// </summary>    
    public virtual void RollbackTransaction()
    {
      prefetchManager.Clear();
    }

  }
}
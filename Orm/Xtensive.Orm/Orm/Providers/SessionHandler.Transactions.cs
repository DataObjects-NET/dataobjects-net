// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.08.19

using System;
using System.Threading.Tasks;
using Xtensive.Orm;


namespace Xtensive.Orm.Providers
{
  partial class SessionHandler
  {
    // Transaction related methods

    /// <summary>
    /// Gets a value indicating whether transaction is actually started.
    /// This property indicates presence of outermost transaction only.
    /// </summary>
    public abstract bool TransactionIsStarted { get; }

    /// <summary>
    /// Opens the transaction.
    /// </summary>
    public abstract void BeginTransaction(Transaction transaction);

    /// <summary>
    /// Clears transaction-related caches.
    /// This method is called for non-actual transactions as well.
    /// </summary>    
    public abstract void CompletingTransaction(Transaction transaction);

    /// <summary>
    /// Commits the transaction.
    /// This method is invoked for actual transactions only.
    /// </summary>    
    public abstract void CommitTransaction(Transaction transaction);

    /// <summary>
    /// Commits the transaction.
    /// This method is invoked for actual transactions only.
    /// </summary>
    public virtual ValueTask CommitTransactionAsync(Transaction transaction)
    {
      CommitTransaction(transaction);
      return default;
    }

    /// <summary>
    /// Rollbacks the transaction.
    /// This method is invoked for actual transactions only.
    /// </summary>
    public abstract void RollbackTransaction(Transaction transaction);

    /// <summary>
    /// Rollbacks the transaction.
    /// This method is invoked for actual transactions only.
    /// </summary>
    public virtual ValueTask RollbackTransactionAsync(Transaction transaction)
    {
      RollbackTransaction(transaction);
      return default;
    }

    // Savepoint related methods

    /// <summary>
    /// Creates the savepoint.
    /// </summary>
    public abstract void CreateSavepoint(Transaction transaction);

    /// <summary>
    /// Releases the savepoint.
    /// </summary>
    public abstract void ReleaseSavepoint(Transaction transaction);

    /// <summary>
    /// Rollbacks to savepoint.
    /// </summary>
    public abstract void RollbackToSavepoint(Transaction transaction);
  }
}
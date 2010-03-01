// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2008.08.30

using System;
using Xtensive.Integrity.Transactions;

namespace Xtensive.Storage
{
  /// <summary>
  /// An implementation of <see cref="Integrity.Transactions.TransactionScope"/>
  /// suitable for storage.
  /// </summary>
  public class TransactionScope : IDisposable
  {
    private static readonly TransactionScope VoidScope = new TransactionScope();

    private bool isCompleted;

    /// <summary>
    /// <see cref="TransactionScope"/> instance that is used for all <see cref="IsVoid">nested</see> scopes.
    /// </summary>
    public static TransactionScope VoidScopeInstance { get { return VoidScope; } }

    /// <summary>
    /// Gets the transaction this scope controls.
    /// </summary>
    public Transaction Transaction { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this scope is void,
    /// i.e. is included into another <see cref="TransactionScope"/> 
    /// and therefore does nothing on opening and disposing.
    /// </summary>
    public bool IsVoid { get { return this==VoidScopeInstance; } }

    /// <summary>
    /// Marks the scope as successfully completed 
    /// (i.e. all operations within the scope are completed successfully).
    /// </summary>
    public void Complete()
    {
      isCompleted = true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
      if (Transaction==null || !Transaction.State.IsActive())
        return;
      if (isCompleted)
        Transaction.Commit();
      else
        Transaction.Rollback();
    }


    // Constructors

    private TransactionScope()
    {
    }

    internal TransactionScope(Transaction transaction)
    {
      Transaction = transaction;
    }
  }
}
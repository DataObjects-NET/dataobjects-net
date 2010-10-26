// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2008.08.30

using System;
using Xtensive.Core;
using Xtensive.Transactions;
using Xtensive.Disposing;

namespace Xtensive.Orm
{
  /// <summary>
  /// An implementation of <see cref="Xtensive.Transactions.TransactionScope"/>
  /// suitable for storage.
  /// </summary>
  public class TransactionScope : CompletableScope
  {
    private static readonly TransactionScope VoidScope = new TransactionScope();

    protected IDisposable disposable;
    protected bool isDisposed;

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

    /// <inheritdoc/>
    public override void Dispose()
    {
      if (isDisposed)
        return;
      isDisposed = true;
      try {
        if (Transaction==null || !Transaction.State.IsActive())
          return;
        if (IsCompleted)
          Transaction.Commit();
        else
          Transaction.Rollback();
      }
      finally {
        try {
          disposable.DisposeSafely(true);
        }
        finally {
          disposable = null;
        }
      }
    }


    // Constructors

    private TransactionScope()
    {
    }

    internal TransactionScope(Transaction transaction, IDisposable disposable)
    {
      Transaction = transaction;
      this.disposable = disposable;
    }
  }
}
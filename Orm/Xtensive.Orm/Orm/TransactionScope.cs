// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2008.08.30

using System;
using System.Threading.Tasks;
using Xtensive.Core;

namespace Xtensive.Orm
{
  /// <summary>
  /// Transaction scope suitable for storage.
  /// </summary>
  public sealed class TransactionScope : ICompletableScope
  {
    private static readonly TransactionScope VoidScope = new TransactionScope();

    private IDisposable disposable;
    private bool isDisposed;

    /// <summary>
    /// Gets a value indicating whether this instance is <see cref="Complete"/>d.
    /// </summary>
    public bool IsCompleted { get; private set; }

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
    /// Completes this scope. 
    /// This method can be called multiple times; if so, only the first call makes sense.
    /// </summary>
    public void Complete()
    {
      IsCompleted = true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
      if (isDisposed)
        return;
      isDisposed = true;
      try {
        if (Transaction==null || !Transaction.State.IsActive())
          return;
        Transaction.Session.CancelAllAsyncQueriesForToken(Transaction.LifetimeToken);
        Transaction.Session.DisposeBlockingCommandsForToken(Transaction.LifetimeToken);
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
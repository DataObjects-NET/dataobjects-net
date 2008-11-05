// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.11.05

using System;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Integrity.Transactions;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage
{
  /// <summary>
  /// An abstract base class for objects having associated transactional state.
  /// </summary>
  public abstract class TransactionalStateContainer : SessionBound
  {
    private Transaction stateTransaction;

    /// <summary>
    /// Gets the transaction where <see cref="TransactionalStateContainer"/>'s 
    /// state was acquired.
    /// </summary>
    [Infrastructure]
    public Transaction StateTransaction {
      get { return stateTransaction; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance's state is actual now.
    /// </summary>
    [Infrastructure]
    protected bool IsStateActual {
      get {
        return stateTransaction!=null && stateTransaction.State.IsActive();
      }
    }

    /// <summary>
    /// Ensures the state is actual. 
    /// If it really is now, this method does nothing.
    /// Otherwise it calls <see cref="ResetState"/> method and sets
    /// <see cref="StateTransaction"/> to <see langword="null" />.
    /// </summary>
    [Infrastructure]
    protected internal void EnsureStateIsActual()
    {
      if (IsStateActual)
        return;
      ResetState();
      stateTransaction = null;
    }

    /// <summary>
    /// Resets the cached transactional state.
    /// </summary>
    [Infrastructure]
    protected abstract void ResetState();

    /// <exception cref="NotSupportedException"></exception>
    [Infrastructure]
    protected void BindStateTransaction()
    {
      var currentTransaction = Session.Transaction;
      if (stateTransaction!=null && stateTransaction!=currentTransaction)
        throw new InvalidOperationException(Strings.ExStateTransactionIsDifferent);
      stateTransaction = currentTransaction;
    }


    // Constructors

    /// <inheritdoc/>
    protected TransactionalStateContainer()
    {
    }

    /// <inheritdoc/>
    protected TransactionalStateContainer(Session session)
      : base(session)
    {
    }
  }
}
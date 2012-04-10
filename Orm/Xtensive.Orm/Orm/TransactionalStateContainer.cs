// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.11.05

using System;
using Xtensive.Aspects;


namespace Xtensive.Orm
{
  /// <summary>
  /// An abstract base class for objects having associated transactional state.
  /// </summary>
  [Infrastructure]
  public abstract class TransactionalStateContainer<TState> : SessionBound
  {
    private TState state;
    private bool isActual;

    /// <summary>
    /// Gets the transaction where container's state was acquired.
    /// </summary>
    public Transaction Transaction { get; private set; }

    /// <summary>
    /// Gets a value indicating whether base state is loaded or not.
    /// </summary>
    public bool IsActual {
      get {
        EnsureIsActual();
        return isActual;
      }
    }

    /// <summary>
    /// Gets the transactional state.
    /// </summary>
    protected TState State {
      get {
        EnsureIsActual();
        if (!isActual)
          Refresh();
        return state;
      }
      set {
        // EnsureIsActual(); - absolutely unnecessary; commented to increase performance
        BindToCurrentTransaction(true);
        isActual = true;
        state = value;
      }
    }

    /// <summary>
    /// Ensures the state is actual. 
    /// If it really is now, this method does nothing.
    /// Otherwise it calls <see cref="Invalidate"/> method.
    /// </summary>
    protected void EnsureIsActual()
    {
      if (!CheckIsActual())
        Invalidate();
    }

    /// <summary>
    /// Resets the cached transactional state.
    /// </summary>
    protected virtual void Invalidate()
    {
      state = default(TState);
      isActual = false;
      Transaction = null;
    }

    /// <summary>
    /// Loads\refreshes the state.
    /// </summary>
    protected abstract void Refresh();
    
    /// <summary>
    /// Binds the the state to the current transaction.
    /// This method must be invoked on state update.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// State is not loaded yet or it is not valid in current transaction.</exception>
    protected void BindToCurrentTransaction()
    {
      BindToCurrentTransaction(false);
    }

    #region Private / internal methods

    private bool CheckIsActual()
    {
      if (Transaction==null)
        return false;
      var currentTransaction = Session.Transaction;
      if (currentTransaction==null)
        return false;
      return Transaction.AreChangesVisibleTo(currentTransaction);
    }

    private void BindToCurrentTransaction(bool skipValidation)
    {
      var currentTransaction = Session.Transaction;
      if (currentTransaction==null)
        throw new InvalidOperationException(Strings.ExTransactionRequired);
      if (!skipValidation)
        if (Transaction==null || !Transaction.AreChangesVisibleTo(currentTransaction))
          throw new InvalidOperationException(Strings.ExCanNotMarkStateAsModifiedItIsNotValidInCurrentTransaction);
      Transaction = currentTransaction;
    }

    #endregion


    // Constructors

    
    protected TransactionalStateContainer(Session session)
      : base(session)
    {
    }
  }
}
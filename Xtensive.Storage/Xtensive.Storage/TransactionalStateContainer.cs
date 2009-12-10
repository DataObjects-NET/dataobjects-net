// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.11.05

using System;
using Xtensive.Core.Aspects;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage
{
  /// <summary>
  /// An abstract base class for objects having associated transactional state.
  /// </summary>
  public abstract class TransactionalStateContainer<TState> : SessionBound
  {
    private TState state;
    private bool stateIsLoaded;

    /// <summary>
    /// Gets the transaction where container's state was acquired.
    /// </summary>
    [Infrastructure]
    public Transaction StateTransaction { get; private set; }

    /// <summary>
    /// Gets a value indicating whether base state is loaded or not.
    /// </summary>
    public bool StateIsLoaded {
      get {
        EnsureStateIsActual();
        return stateIsLoaded;
      }
    }

    /// <summary>
    /// Gets the transactional state.
    /// </summary>
    protected TState State {
      get {
        EnsureStateIsActual();
        if (!stateIsLoaded)
          LoadState();
        return state;
      }
      set {
        EnsureStateIsActual();
        BindStateTransaction();
        stateIsLoaded = true;
        state = value;
      }
    }

    /// <summary>
    /// Ensures the state is actual. 
    /// If it really is now, this method does nothing.
    /// Otherwise it calls <see cref="ResetState"/> method.
    /// </summary>
    protected void EnsureStateIsActual()
    {
      if (!CheckStateIsActual())
        ResetState();
    }

    /// <summary>
    /// Resets the cached transactional state.
    /// </summary>
    protected virtual void ResetState()
    {
      state = default(TState);
      stateIsLoaded = false;
      StateTransaction = null;
    }

    /// <summary>
    /// Loads the state.
    /// </summary>
    protected abstract void LoadState();
    
    /// <summary>
    /// Marks the state as modified.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// State is not loaded yet or it is not valid in current transaction.</exception>
    protected void MarkStateAsModified()
    {
      if (StateTransaction==null || !StateTransaction.AreChangesVisibleTo(Session.Transaction))
        throw new InvalidOperationException(Strings.ExCanNotMarkStateAsModifiedItIsNotValidInCurrentTransaction);
      BindStateTransaction();
    }

    #region Private / internal methods

    private bool CheckStateIsActual()
    {
      if (StateTransaction==null)
        return false;
      var currentTransaction = Session.Transaction;
      if (currentTransaction==null)
        return false;
      return StateTransaction.AreChangesVisibleTo(currentTransaction);
    }

    private void BindStateTransaction()
    {
      var currentTransaction = Session.Transaction;
      if (currentTransaction==null)
        throw new InvalidOperationException(Strings.ExTransactionRequired);
      StateTransaction = currentTransaction;
    }

    #endregion


    // Constructors

    /// <inheritdoc/>
    protected TransactionalStateContainer(Session session)
      : base(session)
    {
    }
  }
}
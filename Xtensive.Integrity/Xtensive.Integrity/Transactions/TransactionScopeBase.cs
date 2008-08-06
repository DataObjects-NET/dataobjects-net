// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.15

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Integrity.Resources;

namespace Xtensive.Integrity.Transactions
{
  /// <summary>
  /// Base class for transaction activation scope
  /// (<see cref="TransactionBase{TScope,TTransaction}"/>).
  /// </summary>
  /// <typeparam name="TScope">Actual scope type.</typeparam>
  /// <typeparam name="TTransaction">Actual transaction type.</typeparam>
  public abstract class TransactionScopeBase<TScope, TTransaction>: Scope<TTransaction>,
    ITransactionScope<TTransaction>
    where TScope: TransactionScopeBase<TScope, TTransaction>
    where TTransaction: TransactionBase<TScope, TTransaction>
  {
    private bool isCompleted;

    /// <summary>
    /// Gets current <see cref="TTransaction"/> object in this type of scope.
    /// </summary>
    [DebuggerHidden]
    public static TTransaction CurrentTransaction {
      get {
        return CurrentContext;
      }
    }

    /// <summary>
    /// Gets <see cref="TTransaction"/> object associated with this scope.
    /// </summary>
    [DebuggerHidden]
    public TTransaction Transaction
    {
      get { return Context; }
    }

    /// <summary>
    /// Indicates that all operations within the scope are completed successfully.
    /// </summary>
    /// <exception cref="InvalidOperationException">This method must be called just once.</exception>
    public void Complete()
    {
      if (isCompleted)
        throw new InvalidOperationException(
          Strings.ExCompleteMustBeCalledJustOnce);
      isCompleted = true;
    }

    /// <inheritdoc/>
    public override void Activate(TTransaction newContext)
    {
      base.Activate(newContext);
      newContext.Activate();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="context">The context.</param>
    public TransactionScopeBase(TTransaction context) 
      : base(context)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    public TransactionScopeBase() 
    {
    }

    // Destructor

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
      try {
        if (isCompleted)
          Transaction.Commit();
        else
          Transaction.Rollback();
      }
      finally {
        base.Dispose(disposing);
      }
    }
  }
}
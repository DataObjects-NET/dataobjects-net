// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.15

using System;
using Xtensive.Internals.DocTemplates;
using Xtensive.Resources;

namespace Xtensive.Transactions
{
  /// <summary>
  /// Transaction activation scope.
  /// </summary>
  public class TransactionScope : IDisposable
  {
    private bool isCompleted;

    /// <summary>
    /// Gets or sets a value indicating whether this scope is successfully completed.
    /// </summary>
    public bool IsCompleted { 
      get { return isCompleted; }
      set {
        if (value==false)
          throw new InvalidOperationException(Strings.ExTransactionScopeIsCompletedCanNotBeSetToFalse);
        isCompleted = true;
      }
    }

    /// <summary>
    /// Gets the transaction this scope controls.
    /// </summary>
    public TransactionBase Transaction { get; private set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="transaction">The transaction this scope controls.</param>
    protected TransactionScope(TransactionBase transaction)
    {
      Transaction = transaction;
    }

    // Destructor

    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    public void Dispose()
    {
      if (Transaction==null)
        return;
      if (IsCompleted)
        Transaction.Commit();
      else
        Transaction.Rollback();
    }
  }
}
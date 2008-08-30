// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.15

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Integrity.Transactions
{
  /// <summary>
  /// Transaction activation scope.
  /// </summary>
  public class TransactionScope : IDisposable
  {
    /// <summary>
    /// Gets a value indicating whether this scope is successfully completed.
    /// </summary>
    public bool IsCompleted { get; private set; }

    /// <summary>
    /// Gets the transaction this scope controls.
    /// </summary>
    public TransactionBase Transaction { get; private set; }

    internal void Complete()
    {
      IsCompleted = true;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    protected TransactionScope(TransactionBase transaction)
    {
      Transaction = transaction;
    }

    // Destructor

    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    public void Dispose()
    {
      if (IsCompleted)
        Transaction.Commit();
      else
        Transaction.Rollback();
    }
  }
}
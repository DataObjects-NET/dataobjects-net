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
  public sealed class TransactionScope : IDisposable
  {
    private bool isCompleted;

    /// <summary>
    /// Gets the transaction.
    /// </summary>
    public TransactionBase Transaction { get; private set;}

    /// <summary>
    /// Indicates that all operations within the scope are completed successfully.
    /// Does nothing if scope is null.
    /// </summary>
    /// <param name="scope">The scope to complete.</param>
    public static void Complete(TransactionScope scope)
    {
      if (scope!=null)
        scope.Complete();
    }

    private void Complete()
    {
      isCompleted = true;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="transaction">The transaction.</param>
    internal TransactionScope(TransactionBase transaction)
    {
      Transaction = transaction;
    }

    // Destructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    /// </summary>
    protected void Dispose(bool disposing)
    {
      if (isCompleted)
        Transaction.Commit();
      else
        Transaction.Rollback();
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
    }
  }
}
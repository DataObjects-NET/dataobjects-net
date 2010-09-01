// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.09.01

using System;
using System.Diagnostics;
using Xtensive.Storage.Resources;
using Xtensive.Integrity.Transactions;
using Xtensive.Core;

namespace Xtensive.Storage
{
  [Serializable]
  public sealed class DiscinnectedStateTransactionScope : TransactionScope
  {
    private readonly Session session;

    /// <inheritdoc/>
    public override void Dispose()
    {
      if (isDisposed)
        return;
      isDisposed = true;
      try {
        if (Transaction == null || !Transaction.State.IsActive())
          return;
        if (IsCompleted)
        {

//          Transaction.Commit();
          
        }
        else
        {
//          Transaction.Rollback();
        }
      }
      finally
      {
        try {
          disposable.DisposeSafely(true);
        }
        finally {
          disposable = null;
        }
      }
    }

    internal DiscinnectedStateTransactionScope(Transaction transaction)
      : base(transaction, transaction.Session.IsDebugEventLoggingEnabled ? Log.DebugRegion(Strings.LogSessionXTransaction, transaction) : null)
    {
      session = transaction.Session;
      session.SystemEvents.NotifyTransactionOpening(transaction);
      session.Events.NotifyTransactionOpening(transaction);
      session.SystemEvents.NotifyTransactionOpened(transaction);
      session.Events.NotifyTransactionOpened(transaction);
    }
  }
}
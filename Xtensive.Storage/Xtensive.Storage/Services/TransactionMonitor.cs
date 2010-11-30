// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2010.11.26

using System;
using System.Collections.Generic;
using Xtensive.Core.Aspects;
using Xtensive.Core.Collections;
using Xtensive.Core.Disposing;
using Xtensive.Core.IoC;
using Xtensive.Core;

namespace Xtensive.Storage.Services
{
  [Service(typeof (TransactionMonitor))]
  [Infrastructure]
  public sealed class TransactionMonitor : SessionBound,
    ISessionService
  {
    private readonly Dictionary<Transaction, SetSlim<IDisposable>> regestry = new Dictionary<Transaction,SetSlim<IDisposable>>();

    public void SetValue(Disposable disposable)
    {
      SetSlim<IDisposable> set;
      if (!regestry.TryGetValue(Session.Transaction, out set)) {
        set = new SetSlim<IDisposable>();
        regestry.Add(Session.Transaction, set);
      }
      set.Add(disposable);
    }

    private void EndTransaction(object sender, TransactionEventArgs e)
    {
      SetSlim<IDisposable> set;
      if (regestry.TryGetValue(Session.Transaction, out set)) {
        regestry.Remove(Session.Transaction);
        foreach (var disposable in set)
          disposable.DisposeSafely();
      }
    }

    // Constructors

    /// <inheritdoc/>
    [ServiceConstructor]
    public TransactionMonitor(Session session)
      : base(session)
    {
      session.SystemEvents.TransactionCommitting += EndTransaction;
      session.SystemEvents.TransactionRollbacking += EndTransaction;
    }
  }
}
// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2010.11.26

using System;
using System.Collections.Generic;
using Xtensive.Aspects;
using Xtensive.Collections;
using Xtensive.Disposing;
using Xtensive.IoC;
using Xtensive.Core;
using Xtensive.Orm;

namespace Xtensive.Orm.Services
{
  [Service(typeof (TransactionMonitor))]
  [Infrastructure]
  public sealed class TransactionMonitor : SessionBound,
    ISessionService
  {
    private readonly Dictionary<Transaction, SetSlim<IDisposable>> registry = new Dictionary<Transaction,SetSlim<IDisposable>>();

    public void SetValue(Disposable disposable)
    {
      SetSlim<IDisposable> set;
      if (!registry.TryGetValue(Session.Transaction, out set)) {
        set = new SetSlim<IDisposable>();
        registry.Add(Session.Transaction, set);
      }
      set.Add(disposable);
    }

    private void EndTransaction(object sender, TransactionEventArgs e)
    {
      SetSlim<IDisposable> set;
      if (registry.TryGetValue(Session.Transaction, out set)) {
        registry.Remove(Session.Transaction);
        foreach (var disposable in set)
          disposable.DisposeSafely();
      }
    }

    // Constructors

    
    [ServiceConstructor]
    public TransactionMonitor(Session session)
      : base(session)
    {
      session.SystemEvents.TransactionCommitting += EndTransaction;
      session.SystemEvents.TransactionRollbacking += EndTransaction;
    }
  }
}
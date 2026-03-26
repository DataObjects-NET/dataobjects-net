// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2010.11.26

using System;
using System.Collections.Generic;
using Xtensive.Collections;

using Xtensive.IoC;
using Xtensive.Core;

namespace Xtensive.Orm.Services
{
  [Service(typeof (TransactionMonitor))]
  public sealed class TransactionMonitor : SessionBound,
    ISessionService
  {
    private readonly Dictionary<Transaction, HashSet<IDisposable>> registry = new Dictionary<Transaction, HashSet<IDisposable>>();

    public void SetValue(Disposable disposable)
    {
      if (!registry.TryGetValue(Session.Transaction, out var set)) {
        set = new HashSet<IDisposable>();
        registry.Add(Session.Transaction, set);
      }
      _ = set.Add(disposable);
    }

    private void EndTransaction(object sender, TransactionEventArgs e)
    {
      if (registry.Remove(Session.Transaction, out var set)) {
        foreach (var disposable in set) {
          disposable.DisposeSafely();
        }
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
// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2019.03.22

using Xtensive.Core;

namespace Xtensive.Orm.Tests
{
  /// <summary>
  /// Provides access to opened session and transaction
  /// </summary>
  internal class TestSessionAccessor : Disposable<Session, TransactionScope>, ITestSessionAccessor
  {
    /// <inheritdoc/>
    public Session Session { get; private set; }

    /// <inheritdoc/>
    public TransactionScope Transaction { get; private set; }

    /// <inheritdoc/>
    public QueryEndpoint Query
    {
      get { return Session.Query; }
    }

    /// <inheritdoc/>
    public void Complete()
    {
      Transaction.Complete();
    }

    private static void DisposeEverything(bool disposing, Session session, TransactionScope transaction)
    {
      transaction.DisposeSafely();
      session.DisposeSafely();
    }

    internal TestSessionAccessor(Session session, TransactionScope transaction)
      : base(session, transaction, DisposeEverything)
    {
    }
  }
}
// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.20

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Integrity.Transactions;

namespace Xtensive.Storage
{
  /// <summary>
  /// Transaction implementation.
  /// </summary>
  public sealed class Transaction : TransactionBase<TransactionScope, Transaction>
  {
    /// <summary>
    /// Gets the session.
    /// </summary>    
    public Session Session { get; private set; }

    /// <inheritdoc/>
    protected override TransactionScope CreateActiveScope()
    {
      return new TransactionScope(this);
    }

    /// <inheritdoc/>
    protected override void OnCommit()
    {
      Session.OnTransactionCommit();
    }

    /// <inheritdoc/>
    protected override void OnRollback()
    {
      Session.OnTransactionRollback();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="session">The session.</param>
    internal Transaction(Session session)
      : this (session, Guid.NewGuid())
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="identifier">The transaction identifier.</param>
    internal Transaction(Session session, Guid identifier)
      : base (identifier)
    {
      Session = session;
    }
  }
}
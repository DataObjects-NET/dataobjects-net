// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.20

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Integrity.Transactions;
using Xtensive.Storage.Rse.Providers.Executable;

namespace Xtensive.Storage
{
  /// <summary>
  /// Transaction implementation.
  /// </summary>
  public sealed class Transaction : TransactionBase
  {
    /// <summary>
    /// Gets the validation context of the transaction.
    /// </summary>    
    public ValidationContext ValidationContext { get; private set; }

    /// <summary>
    /// Gets the session.
    /// </summary>
    public Session Session { get; private set; }

    /// <summary>
    /// Gets the transaction-level temporary data.
    /// </summary>
    public TransactionTemporaryData TemporaryData { get; private set; }


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

    protected override Integrity.Transactions.TransactionScope CreateScope()
    {
      return new TransactionScope(this);
    }

    /// <inheritdoc/>
    protected override void OnActivate()
    {
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
    /// <param name="identifier">The identifier.</param>
    internal Transaction(Session session, Guid identifier)
      : base (identifier)
    {
      Session = session;
      ValidationContext = new ValidationContext();
      TemporaryData = new TransactionTemporaryData();
    }
  }
}
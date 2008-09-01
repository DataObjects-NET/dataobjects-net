// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.20

using System;
using System.Threading;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Integrity.Transactions;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Rse.Providers.Executable;

namespace Xtensive.Storage
{
  /// <summary>
  /// An implementation of transaction suitable for storage.
  /// </summary>
  public sealed class Transaction : TransactionBase
  {
    private IDisposable inconsistentRegion;

    /// <summary>
    /// Gets the session.
    /// </summary>
    public Session Session { get; private set; }

    /// <summary>
    /// Gets the transaction-level temporary data.
    /// </summary>
    public TransactionTemporaryData TemporaryData { get; private set; }

    /// <inheritdoc/>
    protected override Integrity.Transactions.TransactionScope CreateScope()
    {
      return new TransactionScope(this);
    }

    #region OnXxx methods

    /// <inheritdoc/>
    protected override void OnBegin()
    {
      Session.OnTransctionBegin();
      inconsistentRegion = Session.ValidationContext.InconsistentRegion();
    }

    /// <inheritdoc/>
    protected override void OnCommit()
    {
        try {
          inconsistentRegion.Dispose();
        }
        catch {
          OnRollback();
          throw;
        }
        Session.OnTransactionCommit();
    }

    /// <inheritdoc/>
    protected override void OnRollback()
    {
      try {
        Session.ValidationContext.ClearValidationQueue();
        inconsistentRegion.Dispose();
      }
      finally {
        Session.OnTransactionRollback();
      }
    }

    #endregion

    #region Static Current (property), Open (method)

    /// <summary>
    /// Gets the current <see cref="Transaction"/> object
    /// using <see cref="Session"/>.<see cref="Storage.Session.Current"/>.
    /// </summary>
    public static Transaction Current {
      get {
        var session = Session.Current;
        return session!=null ? session.Transaction : null;
      }
    }

    /// <summary>
    /// Does the same as <see cref="Storage.Session.OpenTransaction"/>,
    /// but for the <see cref="Current"/> transaction.
    /// </summary>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object, if new
    /// <see cref="Transaction"/> is created;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no <see cref="Storage.Session.Current"/> <see cref="Session"/>.</exception>
    public static TransactionScope Open()
    {
      var session = Session.Current;
      if (session==null)
        throw new InvalidOperationException(
          Strings.ExCanNotOpenTransactionNoCurrentSession);
      return session.OpenTransaction();
    }

    #endregion


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
      TemporaryData = new TransactionTemporaryData();
    }
  }
} 
// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.20

using System;
using System.Transactions;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Disposing;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Integrity.Transactions;
using Xtensive.Integrity.Validation;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Rse.Providers.Executable;

namespace Xtensive.Storage
{
  /// <summary>
  /// An implementation of transaction suitable for storage.
  /// </summary>
  public sealed class Transaction : TransactionBase,
    IHasExtensions
  {
    private InconsistentRegion inconsistentRegion;
    private ExtensionCollection extensions;

    /// <summary>
    /// Gets the session.
    /// </summary>
    public Session Session { get; private set; }

    /// <summary>
    /// Gets the transaction-level temporary data.
    /// </summary>
    public TransactionTemporaryData TemporaryData { get; private set; }

    /// <summary>
    /// Gets the validation context of this <see cref="Transaction"/>.
    /// </summary>    
    public ValidationContext ValidationContext { get; private set; }

    /// <inheritdoc/>
    protected override Integrity.Transactions.TransactionScope CreateScope()
    {
      return new TransactionScope(this);
    }

    #region IHasExtensions Members

    /// <inheritdoc/>
    public IExtensionCollection Extensions {
      get {
        if (extensions != null)
          return extensions;

        lock (this) {
          if (extensions == null)
            extensions = new ExtensionCollection();
        }

        return extensions;
      }
    }

    #endregion

    #region OnXxx methods

    /// <inheritdoc/>
    protected override void OnBegin()
    {
      ValidationContext.Reset();
      if (Session.Domain.Configuration.ValidationMode==ValidationMode.OnDemand)
        inconsistentRegion = ValidationContext.OpenInconsistentRegion();
      Session.BeginTransaction();
    }

    /// <inheritdoc/>
    protected override void OnCommit()
    {
      try {
        if (inconsistentRegion==null && !ValidationContext.IsConsistent)
          throw new InvalidOperationException(Strings.ExCannotCommitATransactionValidationContextIsInInconsistentState);

        try {
          Validation.Enforce(Session);

          if (inconsistentRegion!=null) {
            inconsistentRegion.Complete();
            inconsistentRegion.DisposeSafely();
          }
        }
        catch (AggregateException exception) {
          throw new InvalidOperationException(Strings.ExCanNotCommitATransactionEntitiesValidationFailed, exception);
        }
      }
      catch {
        OnRollback();
        throw;
      }      
      Session.CommitTransaction();
    }

    /// <inheritdoc/>
    protected override void OnRollback()
    {
      try {
        inconsistentRegion.DisposeSafely();
      }
      finally {
        Session.RollbackTransaction();
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
    /// Opens a new or already running transaction.
    /// </summary>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object, if new <see cref="Transaction"/> is created;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    public static TransactionScope Open()
    {
      return Session.Demand().OpenTransaction();
    }

    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object, if new <see cref="Transaction"/> is created;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    public static TransactionScope Open(IsolationLevel isolationLevel)
    {
      return Session.Demand().OpenTransaction(isolationLevel);
    }

    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object, if new <see cref="Transaction"/> is created;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    public static TransactionScope Open(Session session)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      return session.OpenTransaction();
    }

    /// <summary>
    /// Opens a new or already running transaction.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <returns>
    /// A new <see cref="TransactionScope"/> object, if new <see cref="Transaction"/> is created;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">There is no current <see cref="Session"/>.</exception>
    public static TransactionScope Open(Session session, IsolationLevel isolationLevel)
    {
      ArgumentValidator.EnsureArgumentNotNull(session, "session");
      return session.OpenTransaction(isolationLevel);
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    internal Transaction(Session session, IsolationLevel isolationLevel)
      : this (session, Guid.NewGuid(), isolationLevel)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="identifier">The identifier.</param>
    /// <param name="isolationLevel">The isolation level.</param>
    internal Transaction(Session session, Guid identifier, IsolationLevel isolationLevel)
      : base (identifier, isolationLevel)
    {
      Session = session;
      TemporaryData = new TransactionTemporaryData();
      ValidationContext = new ValidationContext();
    }
  }
} 

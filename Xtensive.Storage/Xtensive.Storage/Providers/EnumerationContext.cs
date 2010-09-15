// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.30

using System;
using Xtensive.Core.Disposing;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Executable;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// An implementation of <see cref="Rse.Providers.EnumerationContext"/> 
  /// suitable for storage.
  /// </summary>
  public sealed class EnumerationContext : Rse.Providers.EnumerationContext
  {
    private readonly EnumerationContextOptions options;

    /// <summary>
    /// Gets the session handler.
    /// </summary>
    /// <value>The session handler.</value>
    public SessionHandler SessionHandler { get; private set; }

    /// <inheritdoc/>
    public override EnumerationContextOptions Options { get { return options; } }

    /// <inheritdoc/>
    public override IDisposable BeginEnumeration()
    {
      var session = SessionHandler.Session;
      var handleAutoTransaction = Transaction.HandleAutoTransaction(session, TransactionalBehavior.Auto);
      session.EnsureTransactionIsStarted();
      return handleAutoTransaction;
    }

    /// <inheritdoc/>
    public override GlobalTemporaryData GlobalTemporaryData {
      get {
        var domain = Domain.Current;
        return domain!=null ? domain.TemporaryData : null;
      }
    }

    /// <inheritdoc/>
    public override TransactionTemporaryData TransactionTemporaryData {
      get {
        var transaction = Transaction.Current;
        return transaction!=null ? transaction.TemporaryData : null;
      }
    }

    /// <inheritdoc/>
    public override Rse.Providers.EnumerationContext CreateNew()
    {
      return new EnumerationContext(SessionHandler, options);
    }

    /// <inheritdoc/>
    protected override Rse.Providers.EnumerationScope CreateActiveScope()
    {
      return new EnumerationScope(this);
    }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="sessionHandler">The session handler.</param>
    /// <param name="options">A value for <see cref="Options"/>.</param>
    public EnumerationContext(SessionHandler sessionHandler, EnumerationContextOptions options)
    {
      SessionHandler = sessionHandler;
      this.options = options;
    }
  }
}
// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.30

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Disposing;

using Xtensive.Orm;
using Xtensive.Orm.Linq.Materialization;
using Xtensive.Orm.Rse.Providers;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// An implementation of <see cref="Xtensive.Orm.Rse.Providers.EnumerationContext"/> 
  /// suitable for storage.
  /// </summary>
  public sealed class EnumerationContext : Rse.Providers.EnumerationContext
  {
    private readonly EnumerationContextOptions options;

    private class EnumerationFinalizer : ICompletableScope
    {
      private readonly Queue<Action> finalizationQueue;
      private readonly TransactionScope transactionScope;

      /// <summary>
      /// Completes this scope.
      /// This method can be called multiple times; if so, only the first call makes sense.
      /// </summary>
      public void Complete()
      {
        if (IsCompleted)
          return;
        IsCompleted = true;
        transactionScope.Complete();
      }

      /// <summary>
      /// Gets a value indicating whether this instance is <see cref="M:Xtensive.Disposing.ICompletableScope.Complete"/>d.
      /// </summary>
      public bool IsCompleted { get; private set; }

      /// <summary>
      /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
      /// </summary>
      public void Dispose()
      {
        while (finalizationQueue.Count > 0) {
          var materializeSelf = finalizationQueue.Dequeue();
          materializeSelf.Invoke();
        }
        transactionScope.DisposeSafely();
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="EnumerationFinalizer"/> class.
      /// </summary>
      /// <param name="finalizationQueue">The finalization queue.</param>
      /// <param name="transactionScope">The transaction scope.</param>
      public EnumerationFinalizer(Queue<Action> finalizationQueue, TransactionScope transactionScope)
      {
        this.finalizationQueue = finalizationQueue;
        this.transactionScope = transactionScope;
      }
    }

    /// <summary>
    /// Gets the session handler.
    /// </summary>
    /// <value>The session handler.</value>
    public SessionHandler SessionHandler { get; private set; }

    internal MaterializationContext MaterializationContext { get; set; }


    /// <summary>
    /// Gets the options of this context.
    /// </summary>
    public override EnumerationContextOptions Options { get { return options; } }


    /// <summary>
    /// Should be called before enumeration of your <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <returns>
    /// An <see cref="IDisposable"/> object.
    /// </returns>
    public override ICompletableScope BeginEnumeration()
    {
      var session = SessionHandler.Session;
      var tx = session.OpenAutoTransaction();
      session.EnsureTransactionIsStarted();
      if (MaterializationContext != null && MaterializationContext.MaterializationQueue != null)
        return new EnumerationFinalizer(MaterializationContext.MaterializationQueue, tx);
      return tx;
    }


    /// <summary>
    /// Gets the global temporary data.
    /// </summary>
    public override GlobalTemporaryData GlobalTemporaryData {
      get {
        var domain = Domain.Current;
        return domain!=null ? domain.TemporaryData : null;
      }
    }


    /// <summary>
    /// Gets the transaction temporary data.
    /// </summary>
    public override TransactionTemporaryData TransactionTemporaryData {
      get {
        var transaction = Transaction.Current;
        return transaction!=null ? transaction.TemporaryData : null;
      }
    }


    /// <summary>
    /// Factory method. Creates new <see cref="EnumerationContext"/>.
    /// </summary>
    /// <returns></returns>
    public override Rse.Providers.EnumerationContext CreateNew()
    {
      return new EnumerationContext(SessionHandler, options);
    }


    /// <summary>
    /// Creates the active scope.
    /// </summary>
    /// <returns></returns>
    protected override Rse.Providers.EnumerationScope CreateActiveScope()
    {
      return new EnumerationScope(this);
    }


    // Constructors

    /// <summary>
    ///   Initializes a new instance of this class.
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